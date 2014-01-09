// Skeleton written by Joe Zachary for CS 3500, September 2013
// implemented by Dylan Noaker
// Read the entire skeleton carefully and completely before you
// do anything else!

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FormulaEvaluator;
using StringExtension;
using System.Globalization;

namespace SpreadsheetUtilities
{
	/// <summary>
	/// Represents formulas written in standard infix notation using standard precedence
	/// rules.  The allowed symbols are non-negative numbers written using double-precision 
	/// floating-point syntax; variables that consist of a letter or underscore followed by 
	/// zero or more letters, underscores, or digits; parentheses; and the four operator 
	/// symbols +, -, *, and /.  
	/// 
	/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
	/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
	/// and "x 23" consists of a variable "x" and a number "23".
	/// 
	/// Associated with every formula are two delegates:  a normalizer and a validator.  The
	/// normalizer is used to convert variables into a canonical form, and the validator is used
	/// to add extra restrictions on the validity of a variable (beyond the standard requirement 
	/// that it consist of a letter or underscore followed by zero or more letters, underscores,
	/// or digits.)  Their use is described in detail in the constructor and method comments.
	/// </summary>
	public class Formula
	{
		/// <summary>
		/// string to hold normalized valid formula
		/// </summary>
		//private string formulaString;
		private List<string> pieces;
		/// <summary>
		/// Creates a Formula from a string that consists of an infix expression written as
		/// described in the class comment.  If the expression is syntactically invalid,
		/// throws a FormulaFormatException with an explanatory Message.
		/// 
		/// The associated normalizer is the identity function, and the associated validator
		/// maps every string to true.  
		/// </summary>
		public Formula(String formula) :
			this(formula, s => s, s => true)
		{
		}

		/// <summary>
		/// Creates a Formula from a string that consists of an infix expression written as
		/// described in the class comment.  If the expression is syntactically incorrect,
		/// throws a FormulaFormatException with an explanatory Message.
		/// 
		/// The associated normalizer and validator are the second and third parameters,
		/// respectively.  If the expression is syntactically correct but contains a variable
		/// v such that isValid(normalize(v)) is false, throws a FormulaFormatException with
		/// an explanatory Message.
		/// 
		/// Suppose that N is a method that converts all the letters in a string to upper case, and
		/// that V is a method that returns true only if a string consists of one letter followed
		/// by one digit.  Then:
		/// 
		/// new Formula("x2+y3", N, V) should succeed
		/// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
		/// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
		/// </summary>
		public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
		{

			//There must be at least one token. it's a good guess to say the string has to have something in it
			if (formula.Length > 0)
			{
				// first, we get the tokens
				//string[] trialTokens = Regex.Split(formula, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
				string[] trialTokens = GetTokens(formula).ToArray();
				//The first token of an expression must be a number, a variable, 
				//or an opening parenthesis. borrowing from get tokens

				if (!(trialTokens[0].IsVar() || trialTokens[0].IsDouble() || trialTokens[0].Equals("(")))
				{
					throw new FormulaFormatException("Error, formula doesn't begin appropriately");
				}
				if (!(trialTokens[trialTokens.Length - 1].IsVar() || 
					trialTokens[trialTokens.Length - 1].IsDouble() || 
					trialTokens[trialTokens.Length - 1].Equals(")")))
				{
					throw new FormulaFormatException("Error, formula doesn't end appropriately");
				}
				//then we can foreach over them
				int openParen = 0;
				int closeParen = 0;
				for (int i = 0; i < trialTokens.Length; i++)
				{
					//is it an op?
					if (trialTokens[i].IsOperator())
					{
						//what is the next thing
						if (i <= trialTokens.Length - 2 && 
							!(trialTokens[i + 1].IsDouble()
						    || trialTokens[i + 1].IsVar()
							|| trialTokens[i + 1] == "("))
						{
							throw new FormulaFormatException("error: unexpected character after operator "+trialTokens[i]);
						}
					}
					//is it an open paren?
					if (trialTokens[i] == "(")
					{
						openParen++;
						//what is the next thing? if it's wrong, we need to throw an exception
						if (i <= trialTokens.Length - 2 && !(trialTokens[i + 1].IsDouble()
						    || trialTokens[i + 1] == ")"
						    || trialTokens[i + 1].IsVar()))
						{
							throw new FormulaFormatException("error: unexpected character after (");
						}
					}
					//is it a close paren?
					if (trialTokens[i] == ")")
					{
						closeParen++;
						//what is the next thing? if it's wrong, we need to throw an exception
						if (i <= trialTokens.Length - 2 &&
							!(trialTokens[i + 1].IsOperator() || trialTokens[i + 1] == ")"))
						{
							throw new FormulaFormatException("error: unexpected character after )");
						}
					}
					//is it a var or double
					if (trialTokens[i].IsVar())
					{
						//what is the next thing
						if (i <= trialTokens.Length - 2 &&
							!(trialTokens[i + 1].IsOperator() || trialTokens[i + 1] == ")"))
						{
							throw new FormulaFormatException("error: unexpected character after var "+trialTokens[i]);
						}
						else
						{
							trialTokens[i] = normalize(trialTokens[i]);
							if (!isValid(trialTokens[i]))
							{
								throw new FormulaFormatException("error: invalid normalized variable.");
							}
						}
						


					}
					if (trialTokens[i].IsDouble())
					{
						//what is the next thing
						if (i <= trialTokens.Length - 2 &&
							!(trialTokens[i + 1].IsOperator() || trialTokens[i + 1] == ")"))
						{
							throw new FormulaFormatException("error: unexpected character after value " + trialTokens[i]);
						}

					}
					//do our open/close parens match?
					

				}//end for loop
				if (closeParen != openParen)
				{
					throw new FormulaFormatException("error: open and close parentheses do not match");
				}
				//--------------------SAVE THE FORMULA AS TOKENS----------------------------\\
				pieces = new List<string>(trialTokens);
				
			}
			else
			{
				//empty string
				throw new FormulaFormatException("error: empty input");
			}
		}
		

		/// <summary>
		/// Evaluates this Formula, using the lookup delegate to determine the values of
		/// variables.  When a variable symbol v needs to be determined, it should be looked up
		/// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
		/// the constructor.)
		/// 
		/// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
		/// in a string to upper case:
		/// 
		/// new Formula("x+7", N, s => true).Evaluate(L) is 11
		/// new Formula("x+7").Evaluate(L) is 9
		/// 
		/// Given a variable symbol as its parameter, lookup returns the variable's value 
		/// (if it has one) or throws an ArgumentException (otherwise).
		/// 
		/// If no undefined variables or divisions by zero are encountered when evaluating 
		/// this Formula, the value is returned. 
		/// -------Otherwise, a FormulaError is sreturned-------.  
		/// The Reason property of the FormulaError should have a meaningful explanation.
		///
		/// --------This method should never throw an exception----------.
		/// </summary>
		public object Evaluate(Func<string, double> lookup)
		{
			try
			{
				return Evaluator.Evaluate(this.ToString(), lookup);
			}
			catch (ArgumentException)
			{
				return new FormulaError("Argument Exception thrown at Evaluator.evaluate()");
			}
			catch (DivideByZeroException)
			{
				return new FormulaError("Error: divide by zero occured");
			}
		}

		/// <summary>
		/// Enumerates the normalized versions of all of the variables that occur in this 
		/// formula.  No normalization may appear more than once in the enumeration, even 
		/// if it appears more than once in this Formula.
		/// 
		/// For example, if N is a method that converts all the letters in a string to upper case:
		/// 
		/// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
		/// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
		/// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
		/// </summary>
		public IEnumerable<String> GetVariables()
		{
			HashSet<string> toreturn = new HashSet<string>();
			foreach (string part in pieces)
			{
				if (Regex.IsMatch(part, @"[a-zA-Z_]([a-zA-Z_]|\d)*"))
				{
					toreturn.Add(part);
				}
			}
			return toreturn.ToList();
		}

		/// <summary>
		/// Returns a string containing no spaces which, if passed to the Formula
		/// constructor, will produce a Formula f such that this.Equals(f).  All of the
		/// variables in the string should be normalized.
		/// 
		/// For example, if N is a method that converts all the letters in a string to upper case:
		/// 
		/// new Formula("x + y", N, s => true).ToString() should return "X+Y"
		/// new Formula("x + Y").ToString() should return "x+Y"
		/// </summary>
		public override string ToString()
		{
			string toreturn = string.Empty;
			foreach (string part in pieces)
			{
				toreturn += part;
			}
			return toreturn;
		}

		/// <summary>
		/// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
		/// whether or not this Formula and obj are equal.
		/// 
		/// Two Formulae are considered equal if they consist of the same tokens in the
		/// same order.  To determine token equality, all tokens are compared as strings 
		/// except for numeric tokens, which are compared as doubles, and variable tokens,
		/// whose normalized forms are compared as strings.
		/// 
		/// For example, if N is a method that converts all the letters in a string to upper case:
		///  
		/// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
		/// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
		/// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
		/// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
		/// </summary>
		public override bool Equals(object obj)
		{
			if(!Object.ReferenceEquals(obj,null))
			{
				if (obj is Formula)
				{
					//rip tokens out of opponent string
					List<string> theirTokens = new List<string>(GetTokens(obj.ToString()));
					// we save our tokens

					int i =-1;
					foreach (string token in theirTokens)
					{
						i++;
					
						double theirDouble = 0.0;
						double ourDouble = 0.0;
						if(Double.TryParse(token,out theirDouble)&&Double.TryParse(pieces[i],out ourDouble))
						{
							if (!(theirDouble == ourDouble))
							{
								return false;
							}
						}
						else if (!token.Equals(pieces[i]))
						{
							return false;
						}

					} //end foreach
					//made it through every element
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Reports whether f1 == f2, using the notion of equality from the Equals method.
		/// Note that if both f1 and f2 are null, this method should return true.  If one is
		/// null and one is not, this method should return false.
		/// </summary>
		public static bool operator ==(Formula f1, Formula f2)
		{
			if (Object.ReferenceEquals(f1, null) && Object.ReferenceEquals(f2, null))
			{
				//both are null
				return true;
			}
			else if (!Object.ReferenceEquals(f1, null))
			{
				return f1.Equals(f2);//false if f2 is null
			}
			//f1 is null
			return false;
		}

		/// <summary>
		/// Reports whether f1 != f2, using the notion of equality from the Equals method.
		/// Note that if both f1 and f2 are null, this method should return false.  If one is
		/// null and one is not, this method should return true.
		/// </summary>
		public static bool operator !=(Formula f1, Formula f2)
		{
			//lean on ==
			return !(f1 == f2);
		}

		/// <summary>
		/// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
		/// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
		/// randomly-generated unequal Formulae have the same hash code should be extremely small.
		/// </summary>
		public override int GetHashCode()
		{
			string ourForm = this.ToString();
			//the string's hashcode plus one of the chars * 31
			return (ourForm.ToString().GetHashCode() + (int)ourForm[(int).75 * ourForm.Length]) * 31;
		}

		/// <summary>
		/// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
		/// right paren; one of the four operator symbols; a string consisting of a letter or underscore
		/// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
		/// match one of those patterns.  There are no empty tokens, and no token contains white space.
		/// </summary>
		private static IEnumerable<string> GetTokens(String formula)
		{
			// Patterns for individual tokens
			String lpPattern = @"\(";
			String rpPattern = @"\)";
			String opPattern = @"[\+\-*/]";
			String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
			String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
			String spacePattern = @"\s+";

			// Overall pattern
			String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
									  lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

			// Enumerate matching tokens that don't consist solely of white space.
			foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
			{
				if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
				{
					yield return s;
				}
			}

		}


	}


	/// <summary>
	/// Used to report syntactic errors in the argument to the Formula constructor.
	/// </summary>
	public class FormulaFormatException : Exception
	{
		/// <summary>
		/// Constructs a FormulaFormatException containing the explanatory message.
		/// </summary>
		public FormulaFormatException(String message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Used as a possible return value of the Formula.Evaluate method.
	/// </summary>
	public struct FormulaError
	{
		/// <summary>
		/// Constructs a FormulaError containing the explanatory reason.
		/// </summary>
		/// <param name="reason"></param>
		public FormulaError(String reason)
			: this()
		{
			Reason = reason;
		}

		/// <summary>
		///  The reason why this FormulaError was created.
		/// </summary>
		public string Reason { get; private set; }
	}
}
namespace StringExtension
{
	/// <summary>
	/// defines handy extensions for string class
	/// </summary>
	public static class StringExtension
	{
		/// <summary>
		/// Determins if instance is a double
		/// </summary>
		/// <param name="s">an instance of string</param>
		/// <returns>bool true if double, else false</returns>
		public static bool IsDouble(this string s)
		{
			//double useless = 0.0;
			//if (Double.TryParse(s, out useless))
			//{
			//	return true;
			//}
			//else
			//{
			//	return false;
			//}//old code 10031639
			double toreturn;
			return (Double.TryParse(s, System.Globalization.NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out toreturn));
		}
		/// <summary>
		/// determines if string is a var
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsVar(this string s)
		{
			return Regex.IsMatch(s, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*");
		}
		/// <summary>
		/// determines if given string is operator
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsOperator(this string s)
		{
			return Regex.IsMatch(s, @"[\+\-*/]");
		}

	}
}

