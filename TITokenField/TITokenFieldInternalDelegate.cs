//
//  TITokenField
//
//  Created by Tom Irving on 16/02/2010.
//  Copyright 2012 Tom Irving. All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:
//
//      1. Redistributions of source code must retain the above copyright notice, this list of
//         conditions and the following disclaimer.
//
//      2. Redistributions in binary form must reproduce the above copyright notice, this list
//         of conditions and the following disclaimer in the documentation and/or other materials
//         provided with the distribution.
//
//  THIS SOFTWARE IS PROVIDED BY TOM IRVING "AS IS" AND ANY EXPRESS OR IMPLIED
//  WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//  FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL TOM IRVING OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
//  ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
//  ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using MonoTouch.UIKit;

namespace TokenField
{
    public class TITokenFieldInternalDelegate : UITextFieldDelegate
    {
        public TITokenFieldInternalDelegate(TITokenField owner)
        {
            this.Owner = owner;
        }

        public TITokenField Owner { get; set; }
        public TITokenFieldDelegate Delegate { get; set; }
        public TITokenField TokenField { get; set; }


        public override bool ShouldBeginEditing(UITextField textField)
        {
            if (this.Delegate != null)
            {
                return this.Delegate.ShouldBeginEditing(textField);
            }
            return true;
        }

        public override void EditingStarted(UITextField textField)
        {
            if (this.Delegate != null)
            {
                this.Delegate.EditingStarted(textField);
            }
            this.Owner.RaiseStarted();
        }

        public override bool ShouldEndEditing(UITextField textField)
        {
            if (this.Delegate != null)
            {
                return this.Delegate.ShouldEndEditing(textField);
            }
            return true;
        }

        public override void EditingEnded(UITextField textField)
        {
            if (this.Delegate != null)
            {
                this.Delegate.EditingEnded(textField);
            }
            this.Owner.RaiseEnded();
        }

        public override bool ShouldReturn(UITextField textField)
        {
            this.TokenField.TokenizeText();
            if (this.Delegate != null)
            {
                return this.Delegate.ShouldReturn(textField);
            }
            return true;
        }

        public override bool ShouldClear(UITextField textField)
        {
            if (this.Delegate != null)
            {
                return this.Delegate.ShouldClear(textField);
            }
            return true;
        }

        public override bool ShouldChangeCharacters(UITextField textField, MonoTouch.Foundation.NSRange range, string replacementString)
        {
            if ((this.TokenField.Tokens.Count > 0) && string.IsNullOrEmpty(replacementString) && (this.TokenField.Text == TITokenField.kTextEmpty))
            {
                this.TokenField.SelectToken(this.TokenField.Tokens[this.TokenField.Tokens.Count - 1]);
                return false;
            }
            if (textField.Text == TITokenField.kTextHidden)
            {
                this.TokenField.RemoveToken(this.TokenField.SelectedToken);
                return !string.IsNullOrEmpty(replacementString);
            }

            if ((replacementString.IndexOfAny(this.TokenField.TokenizingCharacters) >= 0) && !this.TokenField.ForcePickSearchResult)
            {
                this.TokenField.TokenizeText();
                return false;
            }

            if (this.Delegate != null)
            {
                return this.Delegate.ShouldChangeCharacters(textField, range, replacementString);
            }
            return true;
        }
    }
}

