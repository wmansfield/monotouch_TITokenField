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
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using System.Collections;

namespace TokenField
{
    public class TITokenField : UITextField
    {

        #region Static Properties

        public const string kTextEmpty = "\u200B"; // Zero-Width Space
        public const string kTextHidden = "\u200D"; // Zero-Width Joiner

        private object _tokenSyncRoot = new object();

        #endregion

        #region Constructors

        public TITokenField()
        {
            this.Setup();
        }
        public TITokenField (IntPtr handle) : base (handle)
        {
            this.Setup();
        }

        protected virtual void Setup()
        {
            this.Tokens = new List<TIToken>();
            this.Editable = true;
            _maxTokenWidth = 200;
            _tokenTintColor = TIToken.BlueTintColor;
            this.RemovesTokensOnEndEditing = true;
            this.TokenizingCharacters = new char[]{','};

            this.BorderStyle = UITextBorderStyle.None;
            _fontSize = 14;
            this.Font = UIFont.SystemFontOfSize(this.FontSize);
            this.BackgroundColor = UIColor.White;
            this.AutocorrectionType = UITextAutocorrectionType.No;
            this.AutocapitalizationType = UITextAutocapitalizationType.None;
            this.VerticalAlignment = UIControlContentVerticalAlignment.Top;

            this.EditingChanged += Self_EditingChanged;
            this.EditingDidBegin += Self_DidBeginEditing;
            this.EditingDidEnd += Self_EditingDidEnd;

            this.Layer.ShadowColor = UIColor.Black.CGColor;
            this.Layer.ShadowOpacity = 0.6f;
            this.Layer.ShadowRadius = 12;

            this.PromptText = "To:";
            this.Text = kTextEmpty;

            _internalDelegate = new TITokenFieldInternalDelegate(this);
            _internalDelegate.TokenField = this;

            base.WeakDelegate = _internalDelegate;

            _hasSetup = true;// init workaround
            this.Frame = this.Frame;// init workaround
        }

        #endregion

        #region Private Properties

        protected bool _hasSetup; // init workaround
        protected string _promptText;
        protected bool _removesTokensOnEndEditing;
        protected bool _resultsModeEnabled;
        protected TITokenFieldInternalDelegate _internalDelegate;
        protected UILabel _placeHolderLabel;
        protected string _placeHolderText;
        protected PointF _tokenCaret;
        protected float _fontSize;
        protected int _maxTokenWidth;
        protected UIColor _tokenTintColor;

        protected virtual float LeftViewWidth 
        { 
            get
            {
                if (this.LeftView == null
                    || this.LeftViewMode == UITextFieldViewMode.Never
                    || (this.LeftViewMode == UITextFieldViewMode.UnlessEditing && this.IsEditing)
                    || (this.LeftViewMode == UITextFieldViewMode.WhileEditing && !this.IsEditing))
                {
                    return 0;
                }
                return this.LeftView.Bounds.Size.Width;
            }
        }
        protected virtual float RightViewWidth
        { 
            get
            {
                if (this.RightView == null 
                    || this.RightViewMode == UITextFieldViewMode.Never 
                    || (this.RightViewMode == UITextFieldViewMode.UnlessEditing && this.IsEditing)
                    || (this.RightViewMode == UITextFieldViewMode.WhileEditing && !this.IsEditing))
                {
                    return 0;
                }

                return this.RightView.Bounds.Size.Width;
            }
        }

        #endregion

        #region Events

        // replicates protocol for easier implementation [and non breaking for defaults]
        public new event EventHandler Started;
        public new event EventHandler Ended;
        public event EventHandler FrameWillChange;
        public event EventHandler FrameDidChange;
        /// <summary>
        /// Extra event to help layouts that are at the footer, notifies when the field grows in height
        /// </summary>
        public event EventHandler BoundsDidChange;
        public event CancellableProtocolHandler<TITokenField, TIToken> WillAddToken;
        public event CancellableProtocolHandler<TITokenField, TIToken> WillRemoveToken;
        public event ProtocolHandler<TITokenField, TIToken> DidAddToken;
        public event ProtocolHandler<TITokenField, TIToken> DidRemoveToken;
        public event ProtocolHandler<TITokenField, IEnumerable> DidFinishSearch;

        public void RaiseDidFinishSearch(TITokenField tokenField, IEnumerable resultsArray)
        {
            this.DidFinishSearch.Raise(tokenField, resultsArray);
        }
        public void RaiseStarted()
        {
            this.Started.Raise(this, EventArgs.Empty);
        }
        public void RaiseEnded()
        {
            this.Ended.Raise(this, EventArgs.Empty);
        }


        #endregion

        #region Public Properties

        public virtual TIToken SelectedToken { get; protected set; }
        public virtual char[] TokenizingCharacters { get; set; }
        public virtual List<TIToken> Tokens { get; set; }
        public virtual bool Editable { get; set; }
        public virtual bool RemovesTokensOnEndEditing { get; set; }
        public virtual bool ForcePickSearchResult { get; set; }
        public virtual int NumberOfLines { get; protected set; }
        public virtual bool ResultsModeEnabled 
        { 
            get
            {
                return _resultsModeEnabled;
            }
        }
        public virtual int MaxTokenWidth
        {
            get
            {
                return _maxTokenWidth;
            }
            set
            {
                Wrap.Method("TokenField.MaxTokenWidth", delegate()
                {
                    if (_maxTokenWidth != value)
                    {
                        _maxTokenWidth = value;
                        LayoutTokensInternal();
                    }
                });
            }
        }

        public UIColor TokenTintColor
        {
            get
            {
                return _tokenTintColor;
            }
            set
            {
                Wrap.Method("TokenField.TokenTintColor", delegate()
                {
                    if (_tokenTintColor != value)
                    {
                        _tokenTintColor = value;
                        LayoutTokensInternal();
                    }
                });
            }
        }

        public virtual string PromptText
        {
            get
            {
                return _promptText;
            }
            set
            {
                Wrap.Method("TokenField.PromptText",delegate()
                {
                    _promptText = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        UILabel label = this.LeftView as UILabel;
                        if (label == null)
                        {
                            label = new UILabel();
                            label.TextColor = UIColor.FromWhiteAlpha(0.5f, 1f);
                            this.LeftView = label;
                            this.LeftViewMode = UITextFieldViewMode.Always;
                        }

                        label.Text = value;
                        label.Font = UIFont.SystemFontOfSize(this.Font.PointSize + 1);
                        label.SizeToFit();
                    }
                    else
                    {
                        this.LeftView = null;
                    }
                    this.LayoutTokensAnimated(true);
                });
            }
        }
        public virtual string PlaceHolder
        {
            get
            {
                return _placeHolderText;
            }
            set
            {
                Wrap.Method("TokenField.PlaceHolder", delegate()
                {
                    _placeHolderText = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        UILabel label = _placeHolderLabel;
                        if ((label == null) || !(label is UILabel))
                        {
                            label = new UILabel(new RectangleF(_tokenCaret.X + 3, _tokenCaret.Y + 2, 0,0));
                            label.TextColor = UIColor.FromWhiteAlpha(0.75f, 1f);
                            _placeHolderLabel = label;
                            this.AddSubview(_placeHolderLabel);
                        }

                        label.Text = value;
                        label.Font = UIFont.SystemFontOfSize(this.Font.PointSize + 1);
                        label.SizeToFit();

                    }
                    else
                    {
                        if (_placeHolderLabel != null)
                        {
                            _placeHolderLabel.RemoveFromSuperview();
                        }
                        _placeHolderLabel = null;
                    }

                    this.LayoutTokensAnimated(true);
                });
            }
        }


        public override RectangleF Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                Wrap.Method("TokenField.Frame", delegate()
                {
                    if (!_hasSetup)
                    {
                        base.Frame = value;
                        return;
                    }
                    this.FrameWillChange.Raise(this, EventArgs.Empty);
                    base.Frame = value;
                    this.FrameDidChange.Raise(this, EventArgs.Empty);
                    this.Layer.ShadowPath = UIBezierPath.FromRect(this.Bounds).CGPath;
                    this.LayoutTokensAnimated(false);
                });
            }
        }
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = (string.IsNullOrEmpty(value) ? kTextEmpty : value);
            }
        }
        public override UIFont Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                UILabel label = this.LeftView as UILabel;
                if (label != null)
                {
                    this.PromptText = label.Text; // forces font update
                }
            }
        }
        public virtual float FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                _fontSize = value;
                this.Font = this.Font.WithSize(_fontSize);
            }
        }
        public new NSObject WeakDelegate
        {
            get
            {
                return _internalDelegate.Delegate;
            }
            set
            {
                _internalDelegate.Delegate = value as TITokenFieldDelegate;
            }
        }

        public new virtual TITokenFieldDelegate Delegate
        {
            get
            {
                return _internalDelegate.Delegate;
            }
            set
            {
                _internalDelegate.Delegate = value;
            }
        }


        #endregion

        #region Public Methods

        public virtual void SetResultsModeEnabled(bool enabled, bool animated = true)
        {
            Wrap.Method("SetResultsModeEnabled", delegate()
            {
                this.LayoutTokensAnimated(animated);

                if (_resultsModeEnabled != enabled)
                {
                    //Hide / show the shadow
                    this.Layer.MasksToBounds = !enabled;

                    UIScrollView scrollView = this.GetScrollView();
                    scrollView.ScrollsToTop = !enabled;
                    scrollView.ScrollEnabled = !enabled;

                    float offset = ((this.NumberOfLines == 1 || !enabled) ? 0 : _tokenCaret.Y - (float)Math.Floor(this.Font.LineHeight * 4 / 7) + 1);
                    scrollView.SetContentOffset(new PointF(0, this.Frame.Y + offset), animated);
                }

                _resultsModeEnabled = enabled;
            });
        }

        public override bool BecomeFirstResponder()
        {
            return Wrap.Function("BecomeFirstResponder", delegate()
            {
                if (!this.Editable)
                {
                    return false;
                }
                return base.BecomeFirstResponder();
            });
        }
        public override bool BeginTracking(UITouch uitouch, UIEvent uievent)
        {
            return Wrap.Function("BeginTracking", delegate()
            {
                if (SelectedToken != null && uitouch.View == this)
                {
                    this.DeselectSelectedToken();
                }
                return base.BeginTracking(uitouch, uievent);
            });
        }

        public virtual TIToken AddToken(string title, object representedObject = null) 
        {
            return Wrap.Function("AddToken.Text", delegate()
            {
                if (!string.IsNullOrEmpty(title))
                {
                    TIToken token = new TIToken()
                    {
                        Title = title,
                        RepresentedObject = representedObject,
                        Font = this.Font
                    };
                    this.AddToken(token);
                    return token;
                }

                return null;
            });
        }

        public virtual void AddToken(TIToken token)
        {
            Wrap.Method("AddToken.Object", delegate()
            {
                lock(_tokenSyncRoot)
                {
                    bool shouldAdd = true;
                    if (this.Delegate != null)
                    {
                        shouldAdd = this.Delegate.WillAddToken(this, token);
                    }
                    if (shouldAdd)
                    {
                        shouldAdd = this.WillAddToken.Raise(this, token);
                    }

                    if (shouldAdd)
                    {
                        token.TintColor = this.TokenTintColor;
                        token.MaxWidth = this.MaxTokenWidth;
                        this.BecomeFirstResponder();

                        token.TouchDown -= Token_TouchDown; //safety
                        token.TouchUpInside -= Token_TouchUpInside; //safety

                        token.TouchDown += Token_TouchDown;
                        token.TouchUpInside += Token_TouchUpInside;

                        this.AddSubview(token);

                        if (!this.Tokens.Contains(token))
                        {
                            this.Tokens.Add(token);

                            if (this.Delegate != null)
                            {
                                this.Delegate.DidAddToken(this, token);
                            }
                            this.DidAddToken.Raise(this, token);

                            if (_placeHolderLabel != null)
                            {
                                _placeHolderLabel.Hidden = true;
                            }
                        }

                        this.SetResultsModeEnabled(false);
                        this.DeselectSelectedToken();
                    }
                }
            });
        }
        public virtual void RemoveToken(TIToken token) 
        {
            Wrap.Method("RemoveToken", delegate()
            {
                lock(_tokenSyncRoot)
                {
                    if (token == this.SelectedToken)
                    {
                        this.DeselectSelectedToken();
                    }

                    bool shouldRemove = true;
                    if (this.Delegate != null)
                    {
                        shouldRemove = this.Delegate.WillRemoveToken(this, token);
                    }
                    if (this.Delegate != null)
                    {
                        shouldRemove = this.WillRemoveToken.Raise(this, token);
                    }
                    if (shouldRemove)
                    {
                        token.RemoveFromSuperview();
                        this.Tokens.Remove(token);
                        token.TouchDown -= Token_TouchDown;
                        token.TouchUpInside -= Token_TouchUpInside;

                        if (this.Delegate != null)
                        {
                            this.Delegate.DidRemoveToken(this, token);
                        }
                        this.DidRemoveToken.Raise(this, token);

                        this.SetResultsModeEnabled(this.ForcePickSearchResult);
                    }
                }
            });
        }
        public virtual void RemoveAllTokens() 
        {
            Wrap.Method("RemoveAllTokens", delegate()
            {
                TIToken[] tokens = this.Tokens.ToArray();
                foreach (var item in tokens)
                {
                    this.RemoveToken(item);
                }
            });
        }
        public virtual void SelectToken(TIToken token) 
        {
            Wrap.Method("SelectToken", delegate()
            {
                this.DeselectSelectedToken();

                this.SelectedToken = token;
                this.SelectedToken.Selected = true;

                this.BecomeFirstResponder();
                this.Text = kTextHidden;
            });
        }
        public virtual void DeselectSelectedToken() 
        {
            Wrap.Method("DeselectSelectedToken", delegate()
            {
                if (this.SelectedToken != null)
                {
                    this.SelectedToken.Selected = false;
                }
                this.SelectedToken = null;

                this.Text = kTextEmpty;
            });
        }
        public virtual void TokenizeText() 
        {
            Wrap.Method("TokenizeText", delegate()
            {
                bool textChanged = false;

                if ((this.Text != kTextEmpty) && (this.Text != kTextHidden) && !this.ForcePickSearchResult)
                {
                    string trimmed = this.Text.Replace(kTextEmpty, string.Empty).Replace(kTextHidden, string.Empty);
                    string[] tokens = trimmed.Split(this.TokenizingCharacters);
                    foreach (var item in tokens)
                    {
                        this.AddToken(item.Trim());
                        textChanged = true;
                    }
                }

                if (textChanged)
                { 
                    this.SendActionForControlEvents(UIControlEvent.EditingChanged);
                }
            });
        }
        public virtual List<string> GetTokenTitles()
        {
            return Wrap.Function("GetTokenTitles", delegate()
            {
                List<string> result = new List<string>();
                TIToken[] tokens = this.Tokens.ToArray();
                foreach (var item in tokens)
                {
                    if (!string.IsNullOrEmpty(item.Title))
                    {
                        result.Add(item.Title);
                    }   
                }
                return result;
            });
        }
        public virtual List<object> GetTokenObjects()
        {
            return Wrap.Function("GetTokenObjects", delegate()
            {
                List<object> result = new List<object>();
                TIToken[] tokens = this.Tokens.ToArray();
                foreach (var item in tokens)
                {
                    if (item.RepresentedObject != null)
                    {
                        result.Add(item.RepresentedObject);
                    }
                    else if (!string.IsNullOrEmpty(item.Title))
                    {
                        result.Add(item.Title);
                    }
                }
                return result;
            });
        }


        public override RectangleF TextRect(RectangleF forBounds)
        {
            return Wrap.Function("TextRect", delegate()
            {
                if (this.Text == kTextHidden)
                {
                    return new RectangleF(0f, -40f, 0f, 0f);
                }

                RectangleF frame = forBounds;
                frame.Offset(_tokenCaret.X + 2f, _tokenCaret.Y + 3f);
                frame.Width -= (_tokenCaret.X + this.RightViewWidth + 10);
                return frame;
            });
        }
        public override RectangleF EditingRect(RectangleF forBounds)
        {
            return Wrap.Function("EditingRect", delegate()
            {
                return this.TextRect(forBounds);
            });
        }
        public override RectangleF PlaceholderRect(RectangleF forBounds)
        {
            return Wrap.Function("PlaceholderRect", delegate()
            {
                return this.TextRect(forBounds);
            });
        }
        public override RectangleF LeftViewRect(RectangleF forBounds)
        {
            return Wrap.Function("LeftViewRect", delegate()
            {
                return new RectangleF(new PointF(8, (float)Math.Ceiling(this.Font.LineHeight * 4 / 7)), this.LeftView.Bounds.Size);
            });
        }
        public override RectangleF RightViewRect(RectangleF forBounds)
        {
            return Wrap.Function("RightViewRect", delegate()
            {
                return new RectangleF(new PointF(Bounds.Size.Width - this.RightView.Bounds.Size.Width - 6, Bounds.Size.Height - this.RightView.Bounds.Size.Height - 6), this.RightView.Bounds.Size);
            });
        }

        public override string ToString()
        {
            return string.Format("[TITokenField: Prompt = \"{0}\"]", this.PromptText);
        }

        public virtual void LayoutTokensAnimated(bool animated)
        {
            Wrap.Method("LayoutTokensAnimated", delegate()
            {
                float newHeight = this.LayoutTokensInternal();
                if (this.Bounds.Size.Height != newHeight)
                {
                    // Animating this seems to invoke the triple-tap-delete-key-loop-problem-thing™
                    UIView.Animate(
                        (animated ? 0.3 : 0),
                        delegate()  // animation
                        {
                            Wrap.Method("LayoutTokensAnimated.Animate",delegate()
                            {
                                this.Frame = new RectangleF(this.Frame.X, this.Frame.Y, this.Bounds.Size.Width, newHeight);
                                this.SendActionForControlEvents((UIControlEvent)ControlEvents.FrameWillChange);
                            });
                        },
                        delegate() // completion
                        {
                            Wrap.Method("LayoutTokensAnimated.Completion", delegate()
                            {
                                this.SendActionForControlEvents((UIControlEvent)ControlEvents.FrameDidChange);
                                this.BoundsDidChange.Raise(this, EventArgs.Empty);
                            });
                        }
                    );
                }
            });
        }
        #endregion

        #region Protected Methods

        protected virtual float LayoutTokensInternal()
        {
            return Wrap.Function("LayoutTokensInternal", delegate()
            {
                float topMargin = (float)Math.Floor(this.Font.LineHeight * 4 / 7);
                float leftMargin = this.LeftViewWidth + 12f;
                float hPadding = 8f;
                float rightMargin = this.RightViewWidth + hPadding;
                float lineHeight = this.Font.LineHeight + topMargin + 5f;

                this.NumberOfLines = 1;
                _tokenCaret = new PointF(leftMargin, (topMargin - 1));

                TIToken[] tokens = this.Tokens.ToArray();
                foreach (TIToken token in tokens)
                {
                    token.TintColor = this.TokenTintColor;
                    token.Font = this.Font;
                    int maxWidth = (int)(this.Bounds.Size.Width - rightMargin - (this.NumberOfLines > 1 ? hPadding : leftMargin));

                    if(maxWidth > this.MaxTokenWidth)
                    {
                        maxWidth = this.MaxTokenWidth;
                    }
                    token.MaxWidth = maxWidth;

                    if (token.Superview != null)
                    {
                        if (_tokenCaret.X + token.Bounds.Size.Width + rightMargin > this.Bounds.Size.Width)
                        {
                            this.NumberOfLines++;
                            _tokenCaret.X = (this.NumberOfLines > 1 ? hPadding : leftMargin);
                            _tokenCaret.Y += lineHeight;
                        }

                        token.Frame = new RectangleF(_tokenCaret, token.Bounds.Size);
                        _tokenCaret.X += token.Bounds.Size.Width + 4;

                        if (this.Bounds.Size.Width - _tokenCaret.X - rightMargin < 50)
                        {
                            this.NumberOfLines++;
                            _tokenCaret.X = (this.NumberOfLines > 1 ? hPadding : leftMargin);
                            _tokenCaret.Y += lineHeight;
                        }
                    }
                }

                return _tokenCaret.Y + lineHeight;
            });
        }


        protected virtual UIScrollView GetScrollView()
        {
            return this.Superview as UIScrollView;
        }

        #endregion

        #region Event Handlers

        protected virtual void Token_TouchDown(object sender, EventArgs e)
        {
            Wrap.Method("Token_TouchDown", delegate()
            {
                TIToken token = sender as TIToken;
                if (token != null)
                {
                    if (this.SelectedToken != null && this.SelectedToken != token)
                    {
                        this.SelectedToken.Selected = false;
                        this.SelectedToken = null;
                    }
                }
            });
        }

        protected virtual void Token_TouchUpInside(object sender, EventArgs e)
        {
            Wrap.Method("Token_TouchUpInside", delegate()
            {
                TIToken token = sender as TIToken;
                if (token != null)
                {
                    if (this.Editable)
                    { 
                        this.SelectToken(token);
                    }
                }
            });
        }

        protected virtual void Self_DidBeginEditing(object sender, EventArgs e)
        {
            Wrap.Method("Self_DidBeginEditing", delegate()
            {
                TIToken[] tokens = this.Tokens.ToArray();
                foreach (var item in tokens)
                {
                    this.AddToken(item);
                }
            });
        }
        protected virtual void Self_EditingDidEnd(object sender, EventArgs e)
        {
            Wrap.Method("Self_EditingDidEnd", delegate()
            {
                if (this.SelectedToken != null)
                {
                    this.SelectedToken.Selected = false;
                }
                this.SelectedToken = null;

                this.TokenizeText();

                if (this.RemovesTokensOnEndEditing)
                {
                    TIToken[] tokens = this.Tokens.ToArray();
                    foreach (var item in tokens)
                    {
                        item.RemoveFromSuperview();   
                    }

                    string untokenized = kTextEmpty;
                    if (this.Tokens.Count > 0)
                    {
                        List<string> titles = this.GetTokenTitles();

                        untokenized = string.Join(", ", titles);
                        NSString nsString = new NSString(untokenized);

                        SizeF untokSize = nsString.StringSize(UIFont.SystemFontOfSize(this.FontSize));
                        float rightOffset = 0;

                        float availableWidth = this.Bounds.Size.Width - this.LeftViewWidth - this.RightViewWidth;

                        if (this.Tokens.Count > 1 && untokSize.Width > availableWidth)
                        {
                            untokenized = string.Format("{0} recipients", titles.Count);
                        }
                    }
                    this.Text = untokenized;
                }

                this.SetResultsModeEnabled(false);
                if (this.Tokens.Count < 1 && this.ForcePickSearchResult)
                {
                    this.BecomeFirstResponder();
                }
            });
        }
        protected virtual void Self_EditingChanged(object sender, EventArgs e)
        {
            Wrap.Method("Self_EditingChanged", delegate()
            {
                if (string.IsNullOrEmpty(this.Text))
                {
                    this.Text = kTextEmpty;
                    if (_placeHolderLabel != null)
                    {
                        _placeHolderLabel.Hidden = false;
                    }
                }
                else
                {
                    if (_placeHolderLabel != null)
                    {
                        _placeHolderLabel.Hidden = true;
                    }
                }
            });
        }

        #endregion

    }
}




