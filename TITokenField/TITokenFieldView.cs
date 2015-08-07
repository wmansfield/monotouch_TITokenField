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
using UIKit;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using System.Threading.Tasks;

namespace TokenField
{
    public class TITokenFieldView : UIScrollView
    {
        #region Constructors

        public TITokenFieldView()
        {
            this.Setup();
        }
        public TITokenFieldView(UITableView customTableView)
        {
            this.CustomTableView = customTableView;
            this.Setup();
        }
        public TITokenFieldView (IntPtr handle) : base (handle)
        {
            this.Setup();
        }

        protected virtual void Setup()
        {
            _tokenDelegateShim = new TITokenFieldDelegateShim(this);
            _tableViewDelegateShim = new TableViewDelegateShim(this);
            _tableViewDataSource = new TableViewDataSourceShim(this);

            this.BackgroundColor = UIColor.Clear;
            this.DelaysContentTouches = true;
            this.MultipleTouchEnabled = false;

            this.TokenField = new TITokenField()
            {
                Frame = new CGRect(0, 0, this.Bounds.Size.Width, 42)
            };

            this.ShowAlreadyTokenized = false;
            this.SearchSubtitles = true;
            this.ForcePickSearchResult = false;
            this.ResultsArray = new List<object>();

           
            this.TokenField.Started += tokenField_DidBeginEditing;
            this.TokenField.Ended += tokenField_DidEndEditing;
            this.TokenField.EditingChanged += tokenField_TextDidChange;

            this.TokenField.FrameDidChange += tokenField_FrameDidChange;

            this.TokenField.Delegate = _tokenDelegateShim;

            this.AddSubview(this.TokenField);

            nfloat tokenFieldBottom = this.TokenField.Frame.Bottom;

            this.Separator = new UIView(new CGRect(0, tokenFieldBottom, this.Bounds.Size.Width, 1));
            this.Separator.BackgroundColor = UIColor.FromWhiteAlpha(0.7f, 1f);

            // This view is created for convenience, because it resizes and moves with the rest of the subviews.
            this.ContentView = new UIView(new CGRect(0, tokenFieldBottom + 1, this.Bounds.Size.Width, this.Bounds.Size.Height - tokenFieldBottom - 1));
            this.ContentView.BackgroundColor = UIColor.Clear;
            this.AddSubview(this.ContentView);

            UITableView customTable = null;
            if (this.TokenField.Delegate != null)
            {
                customTable = this.TokenField.Delegate.GetCustomTableView(this.TokenField);
            }
            if (customTable != null)
            {
                this.ResultsTable = customTable;
                this.ResultsTable.Delegate = _tableViewDelegateShim;
                this.ResultsTable.DataSource = _tableViewDataSource;
                this.IsCustomResultsTable = true;
            }
            else
            {
                this.AddSubview(this.Separator);

                if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                {
                    UITableViewController tableViewController = new UITableViewController(UITableViewStyle.Plain);
                    tableViewController.TableView.Delegate = _tableViewDelegateShim;
                    tableViewController.TableView.DataSource = _tableViewDataSource;
                    tableViewController.PreferredContentSize = new CGSize(400, 400);

                    this.ResultsTable = tableViewController.TableView;

                    _popoverController = new UIPopoverController(tableViewController);
                }
                else
                {

                    this.ResultsTable =  new UITableView(new CGRect(0, tokenFieldBottom + 1, this.Bounds.Size.Width, 10));
                    this.ResultsTable.SeparatorColor = UIColor.FromWhiteAlpha(0.85f, 1f);
                    this.ResultsTable.BackgroundColor = UIColor.FromRGBA(0.92f, 0.92f, 0.92f ,1f);
                    this.ResultsTable.Delegate = _tableViewDelegateShim;
                    this.ResultsTable.DataSource = _tableViewDataSource;
                    this.ResultsTable.Hidden = true;
                    this.AddSubview(this.ResultsTable);

                    _popoverController = null;
                }

                this.BringSubviewToFront(this.Separator);
            }

            this.BringSubviewToFront(this.TokenField);

            this.UpdateContentSize();

            _hasSetup = true;// init workaround
            this.Frame = this.Frame;// init workaround
        }

        #endregion

        #region Private Properties

        private bool _hasSetup; // work around for monotouch init
        private UIPopoverController _popoverController;
        private bool _forcePickSearchResult;
        private TITokenFieldDelegateShim _tokenDelegateShim;
        private TableViewDelegateShim _tableViewDelegateShim;
        private TableViewDataSourceShim _tableViewDataSource;
        private static object _searchResultRoot = new object();

        #endregion

        #region Public Properties

        public virtual bool ShowAlreadyTokenized { get; set; }
        public virtual bool SearchSubtitles { get; set; }
        public virtual bool ForcePickSearchResult
        {
            get
            {
                return _forcePickSearchResult;
            }
            set
            {
                _forcePickSearchResult = value;
                this.TokenField.ForcePickSearchResult = value;
            }
        }
        public virtual TITokenField TokenField { get; protected set; }
        public virtual UITableView ResultsTable { get; protected set; }
        public virtual bool IsCustomResultsTable { get; protected set; }
        public virtual UIView ContentView { get; protected set; }
        public virtual UIView Separator { get; protected set; }
        public virtual object[] SourceArray { get; set; }
        public virtual List<object> ResultsArray { get; protected set; }
        public Func<string, Task<List<object>>> SearchMethodAsync { get; set; }
        public Action<TITokenFieldView> OnSearchComplete { get; set; }
        public virtual UITableView CustomTableView { get; set; }
        public Func<UITableView, NSIndexPath, object, UITableViewCell> CustomGetCellMethod { get; set; }
        public Func<UITableView, NSIndexPath, object, float> CustomGetCellHeightMethod { get; set; }

        public override CGRect Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                Wrap.Method("TokenFieldView.Frame",delegate()
                {
                    base.Frame = value;
                    if (!_hasSetup)
                    {
                        return;
                    } // init work around
                    nfloat width = value.Size.Width;
                    this.Separator.Frame = new CGRect(this.Separator.Frame.Location, new CGSize(width, this.Separator.Bounds.Size.Height));
                    if(!this.IsCustomResultsTable)
                    {
                        this.ResultsTable.Frame = new CGRect(this.ResultsTable.Frame.Location, new CGSize(width, this.ResultsTable.Bounds.Size.Height));
                    }
                    this.ContentView.Frame = new CGRect(this.ContentView.Frame.Location, new CGSize(width, value.Size.Height - this.TokenField.Frame.Bottom));
                    this.TokenField.Frame = new CGRect(this.TokenField.Frame.Location, new CGSize(width, this.TokenField.Bounds.Size.Height));

                    if ((_popoverController != null) && _popoverController.PopoverVisible)
                    {
                        _popoverController.Dismiss(false);
                        this.PresentPopoverAtTokenFieldCaretAnimated(false);
                    }

                    this.UpdateContentSize();
                    this.SetNeedsLayout();
                });
            }
        }
        public override CGPoint ContentOffset
        {
            get
            {
                return base.ContentOffset;
            }
            set
            {
                base.ContentOffset = value;
                this.SetNeedsLayout();
            }
        }
        public virtual List<string> GetTokenTitles()
        {
            return this.TokenField.GetTokenTitles();
        }

        public override bool CanBecomeFirstResponder
        {
            get
            {
                return this.TokenField.CanBecomeFirstResponder;
            }
        }
       
        #endregion

        #region Public Methods

        public override void LayoutSubviews()
        {
            Wrap.Method("LayoutSubviews",delegate()
            {
                base.LayoutSubviews();

                if(!this.IsCustomResultsTable)
                {
                    nfloat relativeFieldHeight = this.TokenField.Frame.Bottom - this.ContentOffset.Y;
                    nfloat newHeight = this.Bounds.Size.Height - relativeFieldHeight;
                    if (newHeight > -1)
                    {
                        this.ResultsTable.Frame = new CGRect(this.ResultsTable.Frame.Location, new CGSize(this.ResultsTable.Bounds.Size.Width, newHeight));
                    }
                }
            });
        }
        public override bool BecomeFirstResponder()
        {
            return this.TokenField.BecomeFirstResponder();
        }
        public override bool ResignFirstResponder()
        {
            return this.TokenField.ResignFirstResponder();
        }
        public virtual void UpdateContentSize()
        {
            this.ContentSize = new CGSize(this.Bounds.Size.Width, this.ContentView.Frame.Bottom + 1);
        }
        public override string ToString()
        {
            return string.Format("[TITokenFieldView: TokenCount={0}]", this.GetTokenTitles().Count);
        }
        public string GetCurrentSearchText()
        {
            return Wrap.Function("GetCurrentSearchText", delegate()
            {
                string text = this.TokenField.Text;
                if(!string.IsNullOrEmpty(text))
                {
                    text = text.Replace(TITokenField.kTextEmpty,"").Replace(TITokenField.kTextHidden,"");
                };
                return text;
            });
        }

        public virtual void DoPerformSearch()
        {
            Wrap.Method("DoPerformSearch", delegate()
            {
                string text = this.TokenField.Text;
                if(!string.IsNullOrEmpty(text))
                {
                    text = text.Replace(TITokenField.kTextEmpty,"").Replace(TITokenField.kTextHidden,"");
                };
                Task.Run(delegate() 
                {
                    this.ResultsForSearchString(text)
                        .ContinueWith(delegate(Task arg) 
                        {
                            this.BeginInvokeOnMainThread(delegate()
                            {
                                Wrap.Method("Search_Callback", delegate()
                                {
                                    if (this.ForcePickSearchResult)
                                    {
                                        this.SetSearchResultsVisible(true);
                                    }
                                    else
                                    {
                                        this.SetSearchResultsVisible(this.ResultsArray.Count > 0);
                                    }
                                });
                            });
                        });
                });
            });
        }
        public virtual void DoClearResults()
        {
            Wrap.Method("DoClearResults", delegate()
            {
                this.ResultsArray.Clear();
                this.ResultsTable.ReloadData();
            });
        }
        #endregion

        #region Protected Methods



        protected virtual void SetSearchResultsVisible(bool visible)
        {
            Wrap.Method("SetSearchResultsVisible", delegate()
            {
                if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                {
                    if (visible)
                    {
                        this.PresentPopoverAtTokenFieldCaretAnimated(true);
                    }
                    else
                    {
                        if (_popoverController != null)
                        {
                            _popoverController.Dismiss(true);
                        }
                    }
                }
                else
                {
                    if(!this.IsCustomResultsTable)
                    {
                        this.ResultsTable.Hidden = !visible;
                    }
                    this.TokenField.SetResultsModeEnabled(visible);
                }
            });

        }
        protected virtual void PresentPopoverAtTokenFieldCaretAnimated(bool animated)
        {
            Wrap.Method("PresentPopoverAtTokenFieldCaretAnimated", delegate()
            {
                UITextPosition position = this.TokenField.GetPosition(this.TokenField.BeginningOfDocument, 2);
                if (_popoverController != null)
                {
                    _popoverController.PresentFromRect(this.TokenField.GetCaretRectForPosition(position), this.TokenField, UIPopoverArrowDirection.Up, animated);
                }
            });
        }

        protected async Task ResultsForSearchString(string searchString)
        {
            await Wrap.MethodAsync("ResultsForSearchString", async delegate()
            {
                this.InvokeOnMainThread(delegate() 
                {
                    this.ResultsArray.Clear();
                    this.ResultsTable.ReloadData();
                });

                if(string.IsNullOrWhiteSpace(searchString))
                {
                    searchString = string.Empty;
                }
                searchString = searchString.Trim().ToLower();

                if (SearchMethodAsync != null)
                {
                    List<object> results = await SearchMethodAsync(searchString);
                    this.InvokeOnMainThread(delegate() 
                    {
                        lock(_searchResultRoot)
                        {
                            // ensure same search is still pending
                            string currentText = this.TokenField.Text;
                            if(!string.IsNullOrEmpty(currentText))
                            {
                                currentText = currentText.Replace(TITokenField.kTextEmpty,"").Replace(TITokenField.kTextHidden,"");
                            };
                            if(currentText.Equals(searchString, StringComparison.OrdinalIgnoreCase))
                            {
                                this.ResultsArray.Clear();
                                foreach (object item in results) 
                                {
                                    this.ResultsArray.Add(item);
                                }

                                if (this.ResultsArray.Count > 0) 
                                {
                                    this.ResultsTable.ReloadData();
                                }
                                if(this.OnSearchComplete != null)
                                {
                                    this.OnSearchComplete(this);
                                }
                            }
                        }
                    });
                }
                else
                {
                    this.InvokeOnMainThread(delegate() 
                    {
                        Wrap.Method("ResultsForSearchString", delegate()
                        {
                            if(!string.IsNullOrEmpty(searchString) || ForcePickSearchResult)
                            {
                                foreach (var sourceObject in this.SourceArray)
                                {
                                    string title = this._tokenDelegateShim.SearchResultStringForRepresentedObject(this.TokenField, sourceObject);
                                    string subTitle = this._tokenDelegateShim.SearchResultSubtitleForRepresentedObject(this.TokenField, sourceObject);
                                    if (!SearchSubtitles || string.IsNullOrEmpty(subTitle))
                                    {
                                        subTitle = string.Empty;
                                    }

                                    if ((this.ForcePickSearchResult && string.IsNullOrEmpty(searchString)) 
                                        || title.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) > -1
                                        || subTitle.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) > -1)
                                    {
                                        bool shouldAdd = !this.ResultsArray.Contains(sourceObject);

                                        if (shouldAdd && !ShowAlreadyTokenized)
                                        {
                                            foreach (var token in this.TokenField.Tokens) 
                                            {
                                                if (token.RepresentedObject == sourceObject)
                                                {
                                                    shouldAdd = false;
                                                    break;
                                                }
                                            }
                                        }
                                        if(shouldAdd)
                                        {
                                            this.ResultsArray.Add(sourceObject);
                                        }
                                    }
                                }
                            }
                            if (this.ResultsArray.Count > 0) 
                            {
                                this.ResultsArray.Sort(delegate(object l, object r)
                                {
                                    string left = this._tokenDelegateShim.SearchResultStringForRepresentedObject(this.TokenField, l);
                                    string right = this._tokenDelegateShim.SearchResultStringForRepresentedObject(this.TokenField, r);
                                    return left.CompareTo(right);
                                });
                            }

                            if (this.ResultsArray.Count > 0) 
                            {
                                this.ResultsTable.ReloadData();
                            }
                        });
                            
                    });
                }
            });
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (TokenField != null)
                {
                    TokenField.Dispose();
                    TokenField = null;
                }
                CustomGetCellHeightMethod = null;
                CustomGetCellMethod = null;
                CustomTableView = null;
                if (_popoverController != null)
                {
                    _popoverController.Dispose();
                    _popoverController = null;
                }
                if (_tokenDelegateShim != null)
                {
                    _tokenDelegateShim.Dispose();
                    _tokenDelegateShim = null;
                }
                if (_tableViewDelegateShim != null)
                {
                    _tableViewDelegateShim.Dispose();
                    _tableViewDelegateShim = null;
                }
                if (_tableViewDataSource != null)
                {
                    _tableViewDataSource.Dispose();
                    _tableViewDataSource = null;
                }

                if (Separator != null)
                {
                    Separator.Dispose();
                    Separator = null;
                }

                if (ContentView != null)
                {
                    ContentView.Dispose();
                    ContentView = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Event Handlers

        protected virtual void tokenField_DidBeginEditing(object sender, EventArgs args) 
        {
            this.DoClearResults();
        }

        protected virtual void tokenField_DidEndEditing(object sender, EventArgs args) 
        {
            this.DoClearResults();
        }

        protected virtual void tokenField_TextDidChange(object sender, EventArgs args) 
        {
            this.DoPerformSearch();
        }

        protected virtual void tokenField_FrameDidChange(object sender, EventArgs args) 
        {
            Wrap.Method("tokenField_FrameDidChange", delegate()
            {
                nfloat tokenFieldBottom = this.TokenField.Frame.Bottom;
                this.Separator.Frame = new CGRect(new CGPoint(this.Separator.Frame.X, tokenFieldBottom), this.Separator.Bounds.Size);
                if(!this.IsCustomResultsTable)
                {
                    this.ResultsTable.Frame = new CGRect(new CGPoint(this.ResultsTable.Frame.X, (tokenFieldBottom + 1)), this.ResultsTable.Bounds.Size);
                }
                this.ContentView.Frame = new CGRect(new CGPoint(this.ContentView.Frame.X, (tokenFieldBottom + 1)), this.ContentView.Bounds.Size);
            });
        }


        #endregion
       

        #region Shim Classes

        public class TITokenFieldDelegateShim : TITokenFieldDelegate
        {
            public TITokenFieldDelegateShim(TITokenFieldView owner)
            {
                this.Owner = owner;
            }
            public TITokenFieldView Owner { get; set; }

            public override bool ShouldBeginEditing(UITextField textField)
            {
                return true;
            }

            public override void EditingEnded(UITextField textField)
            {

            }
            public override void EditingStarted(UITextField textField)
            {

            }
            public override bool ShouldEndEditing(UITextField textField)
            {
                return true;
            }
            public override bool ShouldChangeCharacters(UITextField textField, NSRange range, string replacementString)
            {
                return true;
            }
            public override bool ShouldClear(UITextField textField)
            {
                return true;
            }
            public override bool ShouldReturn(UITextField textField)
            {
                return true;
            }
            public override string DisplayStringForRepresentedObject(TITokenField tokenField, object representedObject)
            {
                if (tokenField.Delegate != null && (tokenField.Delegate != this))
                {
                    return tokenField.Delegate.DisplayStringForRepresentedObject(tokenField, representedObject);
                }

                return representedObject.ToString();
            }
            public override string SearchResultStringForRepresentedObject(TITokenField tokenField, object representedObject)
            {
                if (tokenField.Delegate != null && (tokenField.Delegate != this))
                {
                    return tokenField.Delegate.SearchResultStringForRepresentedObject(tokenField, representedObject);
                }

                return this.DisplayStringForRepresentedObject(tokenField, representedObject);
            }
            public override string SearchResultSubtitleForRepresentedObject(TITokenField tokenField, object representedObject)
            {
                if (tokenField.Delegate != null && (tokenField.Delegate != this))
                {
                    return tokenField.Delegate.SearchResultSubtitleForRepresentedObject(tokenField, representedObject);
                }

                return null;
            }
            public override UITableView GetCustomTableView(TITokenField tokenField)
            {
                if (this.Owner != null && this.Owner.CustomTableView != null)
                {
                    return this.Owner.CustomTableView;
                }
                if (tokenField.Delegate != null && (tokenField.Delegate != this))
                {
                    return tokenField.Delegate.GetCustomTableView(tokenField);
                }
                return null;
            }
            public override bool DisableScrolling(TITokenField tokenField)
            {
                if (this.Owner != null && this.Owner.CustomTableView != null)
                {
                    return true;
                }
                if (tokenField.Delegate != null && (tokenField.Delegate != this))
                {
                    return tokenField.Delegate.DisableScrolling(tokenField);
                }
                return false;
            }
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.Owner = null;
                }
                base.Dispose(disposing);
            }
        }
        public class TableViewDelegateShim : UITableViewDelegate
        {
            public TableViewDelegateShim(TITokenFieldView owner)
            {
                this.Owner = owner;
            }
            public TITokenFieldView Owner { get; set; }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                object representedObject = this.Owner.ResultsArray[indexPath.Row];

                if (this.Owner != null && this.Owner.CustomGetCellHeightMethod != null)
                {
                    return this.Owner.CustomGetCellHeightMethod(tableView, indexPath, representedObject);
                }
                return 44;
            }
            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                Wrap.Method("RowSelected", delegate()
                {
                    object representedObject = this.Owner.ResultsArray[indexPath.Row];
                    TIToken token = new TIToken()
                    {
                        Title = this.Owner.TokenField.Delegate.DisplayStringForRepresentedObject(this.Owner.TokenField, representedObject),
                        RepresentedObject = representedObject
                    };
                    this.Owner.TokenField.AddToken(token);

                    tableView.DeselectRow(indexPath, true);
                    this.Owner.SetSearchResultsVisible(false);
                });
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.Owner = null;
                }
                base.Dispose(disposing);
            }
        }

        public class TableViewDataSourceShim : UITableViewDataSource
        {
            public TableViewDataSourceShim(TITokenFieldView owner)
            {
                this.Owner = owner;
            }
            public TITokenFieldView Owner { get; set; }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return Wrap.Function("RowsInSection", delegate()
                {
                    if (this.Owner.TokenField.Delegate != null)
                    {
                        this.Owner.TokenField.Delegate.DidFinishSearch(this.Owner.TokenField, this.Owner.ResultsArray);
                    }
                    this.Owner.TokenField.RaiseDidFinishSearch(this.Owner.TokenField, this.Owner.ResultsArray);

                    return this.Owner.ResultsArray.Count;
                });
                    
            }
            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                return Wrap.Function("GetCell", delegate()
                {
                    object representedObject = this.Owner.ResultsArray[indexPath.Row];

                    UITableViewCell cell = null;
                    if (this.Owner != null && this.Owner.CustomGetCellMethod != null)
                    {
                        cell = this.Owner.CustomGetCellMethod(tableView, indexPath, representedObject);
                    }
                    if(cell == null)
                    {
                        string cellIdentifier = "ResultsCell";
                        cell = tableView.DequeueReusableCell(cellIdentifier);
                        string subtitle = this.Owner._tokenDelegateShim.SearchResultSubtitleForRepresentedObject(this.Owner.TokenField, representedObject);

                        if (cell == null)
                        {
                            cell = new UITableViewCell((!string.IsNullOrEmpty(subtitle) ? UITableViewCellStyle.Subtitle : UITableViewCellStyle.Default), cellIdentifier);
                        }
                        if (cell.TextLabel != null)
                        {
                            cell.TextLabel.Text = this.Owner._tokenDelegateShim.SearchResultStringForRepresentedObject(this.Owner.TokenField, representedObject);
                        }
                        if (cell.DetailTextLabel != null)
                        {
                            cell.DetailTextLabel.Text = subtitle;
                        }
                    }
                    return cell;
                });
            }
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.Owner = null;
                }
                base.Dispose(disposing);
            }
        }


        #endregion


    }



}




