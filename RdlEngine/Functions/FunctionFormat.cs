/* ====================================================================
    Copyright (C) 2004-2006  fyiReporting Software, LLC

    This file is part of the fyiReporting RDL project.
	
    This library is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General public License for more details.

    You should have received a copy of the GNU Lesser General public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA

    For additional information, email info@fyireporting.com or visit
    the website www.fyiReporting.com.
*/
// Changed by Open PMS - Dental contributors, 2026-11-16 (package S3b-FMT of the .NET 10 move, decision 24 of docs/specs/net10-plan.md,
// docs/port-notes/S3b-FMT.md): doubles and floats are written as text as .NET Framework wrote them (ODNumberText.ToStringNetFx, AsNetFx).
using System;
using System.Collections;
using System.IO;
using System.Reflection;


using fyiReporting.RDL;
using CodeBase;


namespace fyiReporting.RDL
{
	/// <summary>
	/// Format function: Format(expr, string expr format)
	/// </summary>
	[Serializable]
	internal class FunctionFormat : IExpr
	{
		IExpr _Formatee;	// object to format
		IExpr _Format;		// format string
		/// <summary>
		/// Format an object
		/// </summary>
		public FunctionFormat(IExpr formatee, IExpr format) 
		{
			_Formatee = formatee;
			_Format= format;
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.String;
		}

		// 
		public object Evaluate(Report rpt, Row row)
		{
			return EvaluateString(rpt, row);
		}

		public bool IsConstant()
		{
			if (_Formatee.IsConstant())
				return _Format.IsConstant();

			return false;
		}
	
		public IExpr ConstantOptimization()
		{
			_Formatee = _Formatee.ConstantOptimization();
			_Format = _Format.ConstantOptimization();
			if (_Formatee.IsConstant() && _Format.IsConstant())
			{
				string s = EvaluateString(null, null);
				return new ConstantString(s);
			}

			return this;
		}
		
		public bool EvaluateBoolean(Report rpt, Row row)
		{
			string result = EvaluateString(rpt, row);

			return Convert.ToBoolean(result);
		}
		
		public double EvaluateDouble(Report rpt, Row row)
		{
			string result = EvaluateString(rpt, row);

			return Convert.ToDouble(result);
		}
		
		public decimal EvaluateDecimal(Report rpt, Row row)
		{
			string result = EvaluateString(rpt, row);
			return Convert.ToDecimal(result);
		}

		public string EvaluateString(Report rpt, Row row)
		{
			object o = _Formatee.Evaluate(rpt, row);
			if (o == null)
				return null;
			string format = _Format.EvaluateString(rpt, row);
			if (format == null)
				return ODNumberText.ToStringNetFxIfNumber(o);	// just return string version of object (a double as .NET Framework wrote it, S3b-FMT)

			string result=null;
			try
			{
				result = String.Format("{0:" + format + "}", ODNumberText.AsNetFxIfNumber(o));
			}
			catch 		// invalid format string specified
			{			//    treat as a weak error
				result = ODNumberText.ToStringNetFxIfNumber(o);
			}
			return result;
		}

		public DateTime EvaluateDateTime(Report rpt, Row row)
		{
			string result = EvaluateString(rpt, row);
			return Convert.ToDateTime(result);
		}
	}
}
