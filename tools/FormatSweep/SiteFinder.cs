// Finds the places of one compilation that write a double or float as text, or add a fractional time, with the compiler's types (S3b-FMT,
// docs/port-notes/S3b-FMT.md). A place already routed through the helpers is not found: it no longer calls double.ToString, concatenates a
// double, or calls DateTime.AddHours with a double.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FormatSweep {
	public class SiteFinder {
		private readonly string _project;
		private readonly string _root;
		private readonly SemanticModel _semanticModel;
		private readonly SyntaxTree _syntaxTree;
		public readonly List<Site> ListSites=new List<Site>();

		///<summary>The base library's types whose methods take an object and write it as text (composite formats, writers, debug output).</summary>
		private static readonly string[] _arrayTextTypes=new[] { "System.String","System.Text.StringBuilder","System.IO.TextWriter","System.Console",
			"System.Diagnostics.Debug","System.Diagnostics.Trace","System.Runtime.CompilerServices.FormattableStringFactory" };
		private static readonly Dictionary<string,long> _dictAddScales=new Dictionary<string,long> {
			{ "AddDays",86400000 },{ "AddHours",3600000 },{ "AddMinutes",60000 },{ "AddSeconds",1000 },{ "AddMilliseconds",1 } };
		private static readonly Dictionary<string,long> _dictFromScales=new Dictionary<string,long> {
			{ "FromDays",86400000 },{ "FromHours",3600000 },{ "FromMinutes",60000 },{ "FromSeconds",1000 },{ "FromMilliseconds",1 } };

		public SiteFinder(string project,string root,SemanticModel semanticModel) {
			_project=project;
			_root=root;
			_semanticModel=semanticModel;
			_syntaxTree=semanticModel.SyntaxTree;
		}

		public void Run() {
			SyntaxNode syntaxNodeRoot=_syntaxTree.GetRoot();
			foreach(SyntaxNode syntaxNode in syntaxNodeRoot.DescendantNodes()) {
				switch(syntaxNode) {
					case InvocationExpressionSyntax invocation:
						VisitInvocation(invocation);
						break;
					case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression):
						VisitConcat(binary);
						break;
					case AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.AddAssignmentExpression):
						VisitConcatAssignment(assignment);
						break;
					case InterpolationSyntax interpolation:
						VisitInterpolation(interpolation);
						break;
					case MemberAccessExpressionSyntax memberAccess:
						VisitMethodGroup(memberAccess);
						break;
				}
				if(syntaxNode is ExpressionSyntax expression && IsBoxingContext(expression)) {
					VisitBoxing(expression);
				}
			}
		}

		#region Types
		public static bool IsFloating(ITypeSymbol typeSymbol) {
			if(typeSymbol==null) {
				return false;
			}
			if(typeSymbol.SpecialType==SpecialType.System_Double || typeSymbol.SpecialType==SpecialType.System_Single) {
				return true;
			}
			if(typeSymbol is INamedTypeSymbol named && named.OriginalDefinition.SpecialType==SpecialType.System_Nullable_T) {
				ITypeSymbol typeArg=named.TypeArguments[0];
				return typeArg.SpecialType==SpecialType.System_Double || typeArg.SpecialType==SpecialType.System_Single;
			}
			return false;
		}

		private static bool IsIntegral(ITypeSymbol typeSymbol) {
			if(typeSymbol==null) {
				return false;
			}
			switch(typeSymbol.SpecialType) {
				case SpecialType.System_Byte:
				case SpecialType.System_SByte:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_Char:
					return true;
			}
			if(typeSymbol is INamedTypeSymbol named && named.OriginalDefinition.SpecialType==SpecialType.System_Nullable_T) {
				return IsIntegral(named.TypeArguments[0]);
			}
			return false;
		}

		private ITypeSymbol TypeOf(ExpressionSyntax expression) {
			return _semanticModel.GetTypeInfo(expression).Type;
		}

		private IMethodSymbol MethodOf(ExpressionSyntax expression) {
			SymbolInfo symbolInfo=_semanticModel.GetSymbolInfo(expression);
			if(symbolInfo.Symbol is IMethodSymbol method) {
				return method;
			}
			return symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
		}

		///<summary>The namespace-qualified name without type arguments, and System.String rather than "string".</summary>
		private static readonly SymbolDisplayFormat _symbolDisplayFormatName=new SymbolDisplayFormat(
			typeQualificationStyle:SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
			genericsOptions:SymbolDisplayGenericsOptions.None);

		private static string FullName(ITypeSymbol typeSymbol) {
			return typeSymbol==null ? "" : typeSymbol.ToDisplayString(_symbolDisplayFormatName);
		}

		private static bool IsTextType(INamedTypeSymbol typeSymbol) {
			for(INamedTypeSymbol type=typeSymbol;type!=null;type=type.BaseType) {
				if(_arrayTextTypes.Contains(FullName(type))) {
					return true;
				}
			}
			return false;
		}
		#endregion Types

		#region Visitors
		private void VisitInvocation(InvocationExpressionSyntax invocation) {
			IMethodSymbol method=MethodOf(invocation);
			if(method==null) {
				return;
			}
			string containing=FullName(method.ContainingType);
			//value.ToString(...)
			if(method.Name=="ToString" && !method.IsStatic) {
				ExpressionSyntax expressionReceiver=GetReceiver(invocation,out SimpleNameSyntax name);
				if(expressionReceiver!=null && name!=null && IsFloating(TypeOf(expressionReceiver))) {
					Site site=NewSite(invocation,Categories.TO_STRING);
					site.Format=GetConstantString(invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression);
					site.ListEdits.Add(new Edit(name.Identifier.SpanStart,name.Identifier.Span.Length,"ToStringNetFx"));
					site.NeedsUsingCodeBase=true;
					Add(site);
				}
			}
			if(method.Name=="ToString" && method.IsStatic && containing=="System.Convert" && method.Parameters.Length>=1
				&& (method.Parameters[0].Type.SpecialType==SpecialType.System_Double || method.Parameters[0].Type.SpecialType==SpecialType.System_Single))
			{
				Site site=NewSite(invocation,Categories.CONVERT_TO_STRING);
				SeparatedSyntaxList<ArgumentSyntax> arguments=invocation.ArgumentList.Arguments;
				ExpressionSyntax expressionValue=arguments[0].Expression;
				bool isPrimary=IsPrimary(expressionValue);
				//Convert.ToString(  x  ,provider) -> (x).ToStringNetFx(provider) without touching x itself
				site.ListEdits.Add(new Edit(invocation.SpanStart,expressionValue.SpanStart-invocation.SpanStart,isPrimary ? "" : "("));
				if(arguments.Count==1) {
					site.ListEdits.Add(new Edit(expressionValue.Span.End,invocation.Span.End-expressionValue.Span.End,(isPrimary ? "" : ")")+".ToStringNetFx()"));
				}
				else {
					ExpressionSyntax expressionProvider=arguments[1].Expression;
					site.ListEdits.Add(new Edit(expressionValue.Span.End,expressionProvider.SpanStart-expressionValue.Span.End,(isPrimary ? "" : ")")+".ToStringNetFx("));
				}
				site.NeedsUsingCodeBase=true;
				Add(site);
				return;
			}
			//DateTime/DateTimeOffset.AddX(double)
			if((containing=="System.DateTime" || containing=="System.DateTimeOffset") && _dictAddScales.ContainsKey(method.Name)
				&& method.Parameters.Length==1 && method.Parameters[0].Type.SpecialType==SpecialType.System_Double)
			{
				ExpressionSyntax expressionArgument=invocation.ArgumentList.Arguments[0].Expression;
				if(IsWholeMillis(expressionArgument,_dictAddScales[method.Name])) {
					return;
				}
				GetReceiver(invocation,out SimpleNameSyntax name);
				if(name==null) {
					return;
				}
				Site site=NewSite(invocation,Categories.DATETIME_ADD);
				site.ListEdits.Add(new Edit(name.Identifier.SpanStart,name.Identifier.Span.Length,method.Name+"NetFx"));
				site.NeedsUsingCodeBase=true;
				Add(site);
				return;
			}
			//TimeSpan.FromX(double)
			if(containing=="System.TimeSpan" && _dictFromScales.ContainsKey(method.Name) && method.Parameters.Length==1
				&& method.Parameters[0].Type.SpecialType==SpecialType.System_Double)
			{
				ExpressionSyntax expressionArgument=invocation.ArgumentList.Arguments[0].Expression;
				if(IsWholeMillis(expressionArgument,_dictFromScales[method.Name])) {
					return;
				}
				Site site=NewSite(invocation,Categories.TIMESPAN_FROM);
				if(invocation.Expression is MemberAccessExpressionSyntax memberAccessFrom) {
					site.ListEdits.Add(new Edit(memberAccessFrom.Expression.SpanStart,memberAccessFrom.Expression.Span.Length,"ODTimeSpan"));
					site.NeedsUsingCodeBase=true;
				}
				Add(site);
				return;
			}
			//string.Join/Concat of a sequence of doubles
			if(containing=="System.String" && (method.Name=="Join" || method.Name=="Concat") && method.IsGenericMethod
				&& method.TypeArguments.Length==1 && IsFloating(method.TypeArguments[0]))
			{
				Add(NewSite(invocation,Categories.JOIN));
				return;
			}
			if(containing=="System.String" && (method.Name=="Join" || method.Name=="Concat")) {
				foreach(ArgumentSyntax argument in invocation.ArgumentList.Arguments) {
					ITypeSymbol typeArgument=TypeOf(argument.Expression);
					if(typeArgument is IArrayTypeSymbol arrayType && IsFloating(arrayType.ElementType)) {
						Add(NewSite(invocation,Categories.JOIN));
						return;
					}
				}
			}
			//Arguments: a double handed to a number overload that writes text, or boxed to object.
			bool isTextMethod=IsTextType(method.ContainingType);
			foreach(ArgumentSyntax argument in invocation.ArgumentList.Arguments) {
				ITypeSymbol typeArgument=TypeOf(argument.Expression);
				if(!IsFloating(typeArgument)) {
					continue;
				}
				IParameterSymbol parameter=GetParameter(method,invocation,argument);
				if(parameter==null) {
					continue;
				}
				ITypeSymbol typeParameter=parameter.Type;
				if(parameter.IsParams && typeParameter is IArrayTypeSymbol arrayParams && !(TypeOf(argument.Expression) is IArrayTypeSymbol)) {
					typeParameter=arrayParams.ElementType;
				}
				if(isTextMethod && IsFloating(typeParameter) && (method.Name=="Append" || method.Name=="Insert" || method.Name=="Write"
					|| method.Name=="WriteLine"))
				{
					Site site=NewSite(argument,Categories.APPEND);
					site.Target=containing+"."+method.Name;
					AddWrap(site,argument.Expression,".ToStringNetFx()");
					Add(site);
					continue;
				}
				if(typeParameter.SpecialType==SpecialType.System_Object || typeParameter.TypeKind==TypeKind.Interface
					|| FullName(typeParameter)=="System.ValueType")
				{
					if(isTextMethod) {
						Site site=NewSite(argument,Categories.COMPOSITE);
						site.Target=containing+"."+method.Name;
						site.Format=GetCompositeFormats(invocation,method,argument);
						AddWrap(site,argument.Expression,".AsNetFx()");
						Add(site);
					}
					else {
						Site site=NewSite(argument,Categories.BOXED);
						site.Target=containing+"."+method.Name;
						Add(site);
					}
				}
			}
		}

		private void VisitConcat(BinaryExpressionSyntax binary) {
			IMethodSymbol method=MethodOf(binary);
			if(method==null || FullName(method.ContainingType)!="System.String" || method.Name!="op_Addition") {
				return;
			}
			foreach(ExpressionSyntax operand in new[] { binary.Left,binary.Right }) {
				if(!IsFloating(TypeOf(operand)) || IsConstantTextSame(operand)) {
					continue;
				}
				Site site=NewSite(operand,Categories.CONCAT);
				AddWrap(site,operand,".ToStringNetFx()");
				Add(site);
			}
		}

		private void VisitConcatAssignment(AssignmentExpressionSyntax assignment) {
			if(TypeOf(assignment.Left)?.SpecialType!=SpecialType.System_String) {
				return;
			}
			if(!IsFloating(TypeOf(assignment.Right)) || IsConstantTextSame(assignment.Right)) {
				return;
			}
			Site site=NewSite(assignment.Right,Categories.CONCAT);
			AddWrap(site,assignment.Right,".ToStringNetFx()");
			Add(site);
		}

		private void VisitInterpolation(InterpolationSyntax interpolation) {
			if(!IsFloating(TypeOf(interpolation.Expression))) {
				return;
			}
			if(interpolation.FormatClause==null && IsConstantTextSame(interpolation.Expression)) {
				return;
			}
			Site site=NewSite(interpolation,Categories.INTERPOLATION);
			site.Format=interpolation.FormatClause?.FormatStringToken.ValueText;
			AddWrap(site,interpolation.Expression,".AsNetFx()");
			Add(site);
		}

		private void VisitMethodGroup(MemberAccessExpressionSyntax memberAccess) {
			if(memberAccess.Parent is InvocationExpressionSyntax invocation && invocation.Expression==memberAccess) {
				return;
			}
			if(memberAccess.Name.Identifier.ValueText!="ToString") {
				return;
			}
			IMethodSymbol method=MethodOf(memberAccess);
			if(method==null) {
				return;
			}
			string containing=FullName(method.ContainingType);
			bool isDouble=containing=="System.Double" || containing=="System.Single"
				|| (containing=="System.Convert" && method.Parameters.Length>=1 && IsFloating(method.Parameters[0].Type));
			if(!isDouble && !method.IsStatic && IsFloating(TypeOf(memberAccess.Expression))) {
				isDouble=true;
			}
			if(isDouble) {
				Add(NewSite(memberAccess,Categories.METHOD_GROUP));
			}
		}
		///<summary>An expression whose value is stored or returned as it is: a variable's or property's value, an assignment's right side, an
		///element of an initializer, a returned value. (Arguments are VisitInvocation's.)</summary>
		private static bool IsBoxingContext(ExpressionSyntax expression) {
			SyntaxNode parent=expression.Parent;
			return parent is EqualsValueClauseSyntax
				|| (parent is AssignmentExpressionSyntax assignment && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) && assignment.Right==expression)
				|| parent is InitializerExpressionSyntax
				|| parent is ReturnStatementSyntax
				|| parent is ArrowExpressionClauseSyntax;
		}

		///<summary>Reports a double or float stored as an object (a data row's cell, a grid cell's value, a Tag, an object array): it becomes text
		///later, out of the scan's sight, if at all.</summary>
		private void VisitBoxing(ExpressionSyntax expression) {
			TypeInfo typeInfo=_semanticModel.GetTypeInfo(expression);
			if(!IsFloating(typeInfo.Type) || typeInfo.ConvertedType==null) {
				return;
			}
			ITypeSymbol typeConverted=typeInfo.ConvertedType;
			if(typeConverted.SpecialType!=SpecialType.System_Object && typeConverted.TypeKind!=TypeKind.Interface
				&& typeConverted.TypeKind!=TypeKind.Dynamic && FullName(typeConverted)!="System.ValueType")
			{
				return;
			}
			Site site=NewSite(expression,Categories.BOXED);
			if(expression.Parent is AssignmentExpressionSyntax assignment) {
				ISymbol symbol=_semanticModel.GetSymbolInfo(assignment.Left).Symbol;
				site.Target=symbol==null ? "assignment" : FullName(symbol.ContainingType)+"."+symbol.Name;
			}
			else {
				site.Target=expression.Parent.Kind().ToString();
			}
			Add(site);
		}
		#endregion Visitors

		#region Helpers
		///<summary>The receiver of value.M(...) or value?.M(...), and the name M.</summary>
		private static ExpressionSyntax GetReceiver(InvocationExpressionSyntax invocation,out SimpleNameSyntax name) {
			name=null;
			if(invocation.Expression is MemberAccessExpressionSyntax memberAccess) {
				name=memberAccess.Name;
				return memberAccess.Expression;
			}
			if(invocation.Expression is MemberBindingExpressionSyntax memberBinding) {
				name=memberBinding.Name;
				for(SyntaxNode node=invocation.Parent;node!=null;node=node.Parent) {
					if(node is ConditionalAccessExpressionSyntax conditionalAccess) {
						return conditionalAccess.Expression;
					}
				}
			}
			return null;
		}

		private IParameterSymbol GetParameter(IMethodSymbol method,InvocationExpressionSyntax invocation,ArgumentSyntax argument) {
			if(argument.NameColon!=null) {
				return method.Parameters.FirstOrDefault(x => x.Name==argument.NameColon.Name.Identifier.ValueText);
			}
			int index=invocation.ArgumentList.Arguments.IndexOf(argument);
			if(method.Parameters.Length==0) {
				return null;
			}
			if(index>=method.Parameters.Length) {
				IParameterSymbol parameterLast=method.Parameters[method.Parameters.Length-1];
				return parameterLast.IsParams ? parameterLast : null;
			}
			return method.Parameters[index];
		}

		///<summary>True for an expression that takes ".M()" without parentheses: a name, member access, call, element access, literal,
		///parenthesized expression or this.</summary>
		public static bool IsPrimary(ExpressionSyntax expression) {
			switch(expression) {
				case IdentifierNameSyntax _:
				case GenericNameSyntax _:
				case InvocationExpressionSyntax _:
				case ElementAccessExpressionSyntax _:
				case ParenthesizedExpressionSyntax _:
				case ThisExpressionSyntax _:
				case BaseExpressionSyntax _:
				case PredefinedTypeSyntax _:
					return true;
				case MemberAccessExpressionSyntax memberAccess:
					return memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression);
				case LiteralExpressionSyntax literal:
					return literal.IsKind(SyntaxKind.NumericLiteralExpression);
			}
			return false;
		}

		///<summary>Edits that append the suffix to the expression, with parentheses when it needs them; nothing inside it is touched.</summary>
		private static void AddWrap(Site site,ExpressionSyntax expression,string suffix) {
			if(IsPrimary(expression)) {
				site.ListEdits.Add(new Edit(expression.Span.End,0,suffix));
			}
			else {
				site.ListEdits.Add(new Edit(expression.SpanStart,0,"("));
				site.ListEdits.Add(new Edit(expression.Span.End,0,")"+suffix));
			}
			site.NeedsUsingCodeBase=true;
		}

		private string GetConstantString(ExpressionSyntax expression) {
			if(expression==null) {
				return null;
			}
			Optional<object> optional=_semanticModel.GetConstantValue(expression);
			if(optional.HasValue && optional.Value is string text) {
				return text;
			}
			return null;
		}

		///<summary>For a composite format: the format items of this argument ("{0:c}" gives "c"; none gives "<none>"), comma separated.</summary>
		private string GetCompositeFormats(InvocationExpressionSyntax invocation,IMethodSymbol method,ArgumentSyntax argument) {
			int indexFormat=-1;
			for(int i=0;i<method.Parameters.Length;i++) {
				if(method.Parameters[i].Name=="format" && method.Parameters[i].Type.SpecialType==SpecialType.System_String) {
					indexFormat=i;
					break;
				}
			}
			if(indexFormat<0 || indexFormat>=invocation.ArgumentList.Arguments.Count) {
				return null;
			}
			string format=GetConstantString(invocation.ArgumentList.Arguments[indexFormat].Expression);
			if(format==null) {
				return "?";
			}
			int indexItem=invocation.ArgumentList.Arguments.IndexOf(argument)-indexFormat-1;
			List<string> listFormats=new List<string>();
			for(int i=0;i<format.Length;i++) {
				if(format[i]=='{') {
					if(i+1<format.Length && format[i+1]=='{') {
						i++;
						continue;
					}
					int end=format.IndexOf('}',i);
					if(end<0) {
						break;
					}
					string item=format.Substring(i+1,end-i-1);
					int colon=item.IndexOf(':');
					string indexText=(colon<0 ? item : item.Substring(0,colon)).Split(',')[0].Trim();
					if(int.TryParse(indexText,NumberStyles.None,CultureInfo.InvariantCulture,out int index) && index==indexItem) {
						listFormats.Add(colon<0 ? "<none>" : item.Substring(colon+1));
					}
					i=end;
				}
			}
			return string.Join(",",listFormats);
		}

		///<summary>True for a constant double whose .NET text (shortest round-trip) is the .NET Framework's (G15, G7 for a float): 1.5, 0.25,
		///100. Math.PI is not (3.141592653589793 against 3.14159265358979).</summary>
		private bool IsConstantTextSame(ExpressionSyntax expression) {
			Optional<object> optional=_semanticModel.GetConstantValue(expression);
			if(!optional.HasValue) {
				return false;
			}
			if(optional.Value is double valueDouble) {
				return valueDouble.ToString(CultureInfo.InvariantCulture)==valueDouble.ToString("G15",CultureInfo.InvariantCulture)
					&& !(valueDouble==0 && double.IsNegative(valueDouble));
			}
			if(optional.Value is float valueFloat) {
				return valueFloat.ToString(CultureInfo.InvariantCulture)==valueFloat.ToString("G7",CultureInfo.InvariantCulture)
					&& !(valueFloat==0 && float.IsNegative(valueFloat));
			}
			return optional.Value is int || optional.Value is long;
		}

		///<summary>True when the argument of AddX or FromX is a whole number by its type (an integral value converted to double), or a constant
		///whose milliseconds are a whole number: the .NET Framework's rounding to the millisecond changes nothing for it.</summary>
		private bool IsWholeMillis(ExpressionSyntax expression,long scale) {
			if(IsIntegral(TypeOf(expression))) {
				return true;
			}
			Optional<object> optional=_semanticModel.GetConstantValue(expression);
			if(optional.HasValue) {
				double value;
				try {
					value=Convert.ToDouble(optional.Value,CultureInfo.InvariantCulture);
				}
				catch(Exception) {
					return false;
				}
				double millis=value*scale;
				return !double.IsNaN(millis) && !double.IsInfinity(millis) && Math.Floor(millis)==millis;
			}
			return false;
		}

		private Site NewSite(SyntaxNode syntaxNode,string category) {
			FileLinePositionSpan lineSpan=syntaxNode.GetLocation().GetLineSpan();
			string code=syntaxNode.ToString();
			code=string.Join(" ",code.Split(new[] { '\r','\n','\t' },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
			if(code.Length>240) {
				code=code.Substring(0,240)+"...";
			}
			Site site=new Site();
			site.Project=_project;
			site.FilePath=_syntaxTree.FilePath;
			site.RelativePath=GetRelativePath(_root,_syntaxTree.FilePath);
			site.Line=lineSpan.StartLinePosition.Line+1;
			site.Column=lineSpan.StartLinePosition.Character+1;
			site.Category=category;
			site.Code=code;
			return site;
		}

		public static string GetRelativePath(string root,string path) {
			string relative=System.IO.Path.GetRelativePath(root,path);
			return relative.Replace('/','\\');
		}

		private void Add(Site site) {
			ListSites.Add(site);
		}
		#endregion Helpers
	}
}
