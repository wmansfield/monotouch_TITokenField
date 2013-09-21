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
using MonoTouch.Foundation;
using System.Collections;

namespace TokenField
{
    public abstract class TITokenFieldDelegate : UITextFieldDelegate
    {
        public TITokenFieldDelegate()
        {
        }

        public virtual bool WillAddToken(TITokenField tokenField, TIToken token)
        {
            return true;
        }
        public virtual void DidAddToken(TITokenField tokenField, TIToken token)
        {
        }
        public virtual bool WillRemoveToken(TITokenField tokenField, TIToken token)
        {
            return true;
        }
        public virtual void DidRemoveToken(TITokenField tokenField, TIToken token)
        {
        }

        public virtual void DidFinishSearch(TITokenField tokenField, IEnumerable matches)
        {
        }

        public virtual float GetHeightForRow(TITokenField tokenField, UITableView tableView, NSIndexPath indexPath)
        {
            return tableView.RowHeight;
        }
        public virtual UITableViewCell CellForRepresentedObject(TITokenField tokenField, UITableView tableView, object representedObject)
        {
            return new UITableViewCell(UITableViewCellStyle.Default, "tokenFieldDefaultCell");
        }
        public virtual string SearchResultSubtitleForRepresentedObject(TITokenField tokenField, object representedObject)
        {
            if (representedObject != null)
            {
                return representedObject.ToString();
            }
            return string.Empty;
        }
        public virtual string SearchResultStringForRepresentedObject(TITokenField tokenField, object representedObject)
        {
            if (representedObject != null)
            {
                return representedObject.ToString();
            }
            return string.Empty;
        }
        public virtual string DisplayStringForRepresentedObject(TITokenField tokenField, object representedObject)
        {
            if (representedObject != null)
            {
                return representedObject.ToString();
            }
            return string.Empty;
        }

    }
}



