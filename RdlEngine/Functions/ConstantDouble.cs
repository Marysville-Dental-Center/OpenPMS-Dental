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
using System.Globalization;


using fyiReporting.RDL;
using CodeBase;


namespace fyiReporting.RDL
{
	/// <summary>
	/// Constant Double
	/// </summary>
	[Serializable]
	internal class ConstantDouble : IExpr
	{
		double _Value;		// value of the constant

		/// <summary>
		/// passed class name, function name, and args for evaluation
		/// </summary>
		public ConstantDouble(string v) 
		{
			_Value = Convert.ToDouble(v, NumberFormatInfo.InvariantInfo);
		}

		public ConstantDouble(double v) 
		{
			_Value = v;
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Double;
		}

		public bool IsConstant()
		{
			return true;
		}

		public IExpr ConstantOptimization()
		{	// already constant expression
			return this;
		}

		public object Evaluate(Report rpt, Row row)
		{
			return _Value;
		}

		public string EvaluateString(Report rpt, Row row)
		{
			return _Value.ToStringNetFx();
		}
		
		public double EvaluateDouble(Report rpt, Row row)
		{
			return _Value;
		}
		
		public decimal EvaluateDecimal(Report rpt, Row row)
		{
			return Convert.ToDecimal(_Value);
		}

		public DateTime EvaluateDateTime(Report rpt, Row row)
		{
			return Convert.ToDateTime(_Value);
		}

		public bool EvaluateBoolean(Report rpt, Row row)
		{
			return Convert.ToBoolean(_Value);
		}
	}
}
