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

namespace Libraries.LexicalAnalysis
{
    public class TypedSegment : Segment
    {
        public string Type { get; set; }
        public TypedSegment(int length, string type, int offset)
            : base(length, offset)
        {
            Type = type;
        }
        public TypedSegment(int length, string type)
            : this(length, type, 0)
        {
        }
        public TypedSegment(int length)
        : this(length, string.Empty)
        {
        }
        

        public TypedSegment(TypedSegment seg)
            : base(seg)
        {
            Type = seg.Type;
        }
        public TypedSegment(Segment seg, string type)
            : base(seg)
        {
            Type = type;
        }

        public override object Clone()
        {
            return new TypedSegment(this);
        }

        public override bool Equals(object other)
        {
            TypedSegment seg = (TypedSegment)other;
            return seg.Type.Equals(Type) && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() + Type.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}:{1}=>{2}]", Type, Start, Length);
        }       
    }
}
