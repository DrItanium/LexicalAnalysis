//Copyright 2012 Joshua Scoggins. All rights reserved.
//
//Redistribution and use in source and binary forms, with or without modification, are
//permitted provided that the following conditions are met:
//
//   1. Redistributions of source code must retain the above copyright notice, this list of
//      conditions and the following disclaimer.
//
//   2. Redistributions in binary form must reproduce the above copyright notice, this list
//      of conditions and the following disclaimer in the documentation and/or other materials
//      provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY Joshua Scoggins ``AS IS'' AND ANY EXPRESS OR IMPLIED
//WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Joshua Scoggins OR
//CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
//ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
//ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//The views and conclusions contained in the software and documentation are those of the
//authors and should not be interpreted as representing official policies, either expressed
//or implied, of Joshua Scoggins. 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Libraries.Collections;
using Libraries.Extensions;

namespace Libraries.LexicalAnalysis
{
	public class TokenShakerContainer<T> : ShakerContainer<T>
	{
		private TypedShakeSelector<T> selector;
		public new TypedShakeSelector<T> Selector
		{
			get
			{
				return selector;
			}
			set
			{
				this.selector = value;
				base.Selector = (x, y) => CompatibilitySelector(x, y, selector);

			}
		}
		private static Tuple<Hunk<T>, Hunk<T>> CompatibilitySelector(Segment seg, Hunk<T> hunk, TypedShakeSelector<T> selector)
		{
			var r = selector(new TypedSegment(seg, string.Empty), new Token<T>(hunk));
			return new Tuple<Hunk<T>, Hunk<T>>(r.Item1, r.Item2);
		}
		public TokenShakerContainer(TypedShakeSelector<T> selector)
			: base(null) //compat 
		{
			Selector = selector;
		}
		public IEnumerable<Token<T>> TypedShake(Token<T> target, TypedShakeCondition<T> cond)
		{
			return TypedShake(target, cond, Selector);
		}
		public IEnumerable<Token<T>> TypedShake(Token<T> target, TypedShakeCondition<T> cond, TypedShakeSelector<T> selector)
		{
			Token<T> prev = target;
			Tuple<Token<T>, Token<T>, Token<T>> curr = new Tuple<Token<T>, Token<T>, Token<T>>(null, null, target);
			do
			{
				curr = BasicTypedShakerFunction(curr.Item3, cond, selector);
				if (curr.Item1 != null)
				{
					if(!curr.Item1.IsBig)
						curr.Item1.IsBig = true;
					yield return curr.Item1;
				}
				if (curr.Item2 != null)
					yield return curr.Item2;
				if (curr.Item3.Equals((Hunk<T>)prev))
				{
					
					if (curr.Item3.Length > 0)
					{
						if(!curr.Item3.IsBig)
							curr.Item3.IsBig = true;
						yield return curr.Item3;
					}
					yield break;
				}
				prev = curr.Item3;
			} while (true);
		}
		public IEnumerable<Token<T>> TypedShake(Token<T> target, TypedShakeCondition<T> a, TypedShakeCondition<T> b)
		{
			foreach (var v in TypedShake(target, a))
			{
				if (v.IsBig)
					foreach (var q in TypedShake(v, b))
						yield return q;
				else
					yield return v;
			}
		}
		private IEnumerable<Token<T>> TypedShakeInternal(IEnumerable<Token<T>> outer, TypedShakeCondition<T> cond)
		{
			if (cond == null)
			{
				foreach (var v in outer)
					yield return v;
			}
			else
			{
				foreach (var v in outer)
				{
					if (v.IsBig)
					{
						
						foreach (var q in TypedShake(v, cond))
							yield return q;
					}
					else
						yield return v;
				}
			}
		}
		private IEnumerable<Token<T>> TypedShakeSingle(Token<T> tok) 
		{
			return new Token<T>[] { tok };
		}
		public IEnumerable<Token<T>> TypedShake(Hunk<T> input, IEnumerable<TypedShakeCondition<T>> conds)
		{
			return TypedShake(new Token<T>(input), conds);
		}
		public IEnumerable<Token<T>> TypedShake(Token<T> target, IEnumerable<TypedShakeCondition<T>> conds)
		{
			IEnumerable<Token<T>> initial = TypedShake(target, conds.First()).ToArray();
			foreach (var cond in conds.Skip(1))
				initial = TypedShakeInternal(initial, cond);
			return TypedShakeInternal(initial, null);
		}

		public Tuple<Token<T>, Token<T>, Token<T>> BasicTypedShakerFunction(Token<T> target, TypedShakeCondition<T> cond)
		{
			return BasicTypedShakerFunction(target, cond, selector);
		}
		protected Tuple<Token<T>, Token<T>, Token<T>> BasicTypedShakerFunction(
				Token<T> target,
				TypedShakeCondition<T> cond,
				TypedShakeSelector<T> selector)
		{
			Func<TypedSegment, Token<T>, int> getRest = (seg, hunk) => (hunk.Length - (seg.Start + seg.Length));
			Func<TypedSegment, int> getComputedStart = (seg) => seg.Start + seg.Length;

			TypedSegment result = cond(target);
			if (result == null)
			{
				return new Tuple<Token<T>, Token<T>, Token<T>>(null, null, target);
			}
			else
			{
				TypedSegment restSection = new TypedSegment(getRest(result, target), string.Empty, getComputedStart(result));
				var matchTokens = selector(result, target);
				var before = matchTokens.Item1;
				var match = matchTokens.Item2;
				match.IsBig = false;
				var restTokens = selector(restSection, target);
				var rest = restTokens.Item2;
				rest.IsBig = true;
				return new Tuple<Token<T>, Token<T>, Token<T>>(before, match, rest);
			}
		}
	}
	public delegate Tuple<Token<T>, Token<T>> TypedShakeSelector<T>(TypedSegment seg, Token<T> token);
	public delegate TypedSegment TypedShakeCondition<T>(Token<T> hunk);
	public delegate Tuple<Token<T>, Token<T>, Token<T>> TypedShaker<T>(Hunk<T> target,
			TypedShakeCondition<T> condition, TypedShakeSelector<T> selector);
}
