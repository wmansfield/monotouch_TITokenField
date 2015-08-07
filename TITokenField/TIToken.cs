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
using CoreGraphics;
using CoreGraphics;
using Foundation;

namespace TokenField
{
    public class TIToken : UIControl
    {
        #region Static Properties

        protected const float hTextPadding = 14f;
        protected const float vTextPadding = 8f;
        protected const float kDisclosureThickness = 2.5f;
        protected const UILineBreakMode kLineBreakMode = UILineBreakMode.TailTruncation;

        public static UIColor BlueTintColor 
        {
            get
            {
                return UIColor.FromRGBA(0.216f, 0.373f, 0.965f, 1f);
            }
        }
        public static UIColor RedTintColor 
        {
            get
            {
                return UIColor.FromRGBA(1f, 0.15f, 0.15f, 1f);
            }
        }
        public static UIColor GreenTintColor 
        {
            get
            {
                return UIColor.FromRGBA(0.333f, 0.741f, 0.235f, 1f);
            }
        }
        public static UIFont DefaultFont 
        {
            get
            {
                return UIFont.SystemFontOfSize(DefaultFontSize);
            }
        }
        public static float DefaultFontSize
        {
            get
            {
                return 14f;
            }
        }

        #endregion

        #region Constructors

        public TIToken()
        {
            this.Setup();
        }
        public TIToken(IntPtr handle) : base (handle)
        {
            this.Setup();
        }

        protected virtual void Setup()
        {
            this.TokenRadius = 10;
            this.TintColor = TIToken.BlueTintColor;
            this.TextColor = UIColor.Black;
            this.HighlightedTextColor = UIColor.White;
            this.BackgroundColor = UIColor.Clear;
            this.SizeToFit();
        }

        #endregion

        #region Private Properties

        private string _title = string.Empty;
        private int _maxWidth = 200;
        private UIFont _font = TIToken.DefaultFont;
        private AccessoryType _accessoryType = AccessoryType.None;

        #endregion

        #region Public Properties

        // New Props

        public virtual float TokenRadius { get; set; }
        public virtual object RepresentedObject { get; set; }
        public virtual UIColor HighlightedTextColor { get; set; }
        public virtual UIColor TextColor { get; set; }
        public virtual int MaxWidth
        {
            get
            {
                return _maxWidth;
            }
            set
            {
                if (_maxWidth != value)
                {
                    _maxWidth = value;
                    this.SizeToFit();
                }
            }
        }
        public virtual AccessoryType AccessoryType
        {
            get
            {
                return _accessoryType;
            }
            set
            {
                if (_accessoryType != value)
                {
                    _accessoryType = value;
                    this.SizeToFit();
                }
            }
        }
        public virtual string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    this.SizeToFit();
                }
            }
        }
        public virtual UIFont Font
        {
            get
            {
                if (_font == null)
                {
                    return TIToken.DefaultFont;
                }
                return _font;
            }
            set
            {

                if (_font != value)
                {
                    _font = value;
                    this.SizeToFit();
                }
            }
        }

        // Overrides
        public override bool Highlighted
        {
            get
            {
                return base.Highlighted;
            }
            set
            {
                if (base.Highlighted != value)
                {
                    base.Highlighted = value;
                    this.SetNeedsDisplay();
                }
            }
        }
        public override bool Selected
        {
            get
            {
                return base.Selected;
            }
            set
            {
                if (base.Selected != value)
                {
                    base.Selected = value;
                    this.SetNeedsDisplay();
                }
            }
        }
        public override UIColor TintColor
        {
            get
            {
                if (base.TintColor == null)
                {
                    return TIToken.BlueTintColor;
                }
                return base.TintColor;
            }
            set
            {
                if (base.TintColor != value)
                {
                    base.TintColor = value;
                    this.SetNeedsDisplay();
                }
            }
        }


        #endregion

        #region Overrides

        public override void SizeToFit()
        {
            Wrap.Method("SizeToFit", delegate()
            {
                nfloat accessoryWidth = 0;

                if (this.AccessoryType == AccessoryType.DisclosureIndicator)
                {
                    this.CreateDisclosureIndicatorPath(CGPoint.Empty, this.Font.PointSize, kDisclosureThickness, out accessoryWidth);
                    accessoryWidth += (float)Math.Floor((double)hTextPadding / 2);
                }

                CGSize titleSize = new NSString(this.Title).StringSize(this.Font, (this.MaxWidth - hTextPadding - accessoryWidth), kLineBreakMode);
                float height = (float)Math.Floor((double)(titleSize.Height + vTextPadding));
                float width = Math.Max((float)Math.Floor((double)(titleSize.Width + hTextPadding + accessoryWidth)), height - 3);
                this.Frame = new CGRect(this.Frame.X, this.Frame.Y, width, height);
                this.SetNeedsDisplay();
            });
        }
        public override void Draw(CGRect rect)
        {
            Wrap.Method("Draw", delegate()
            {
                CGContext context = UIGraphics.GetCurrentContext();

                // Draw the outline.
                context.SaveState();
                CGPath outlinePath = this.CreateTokenPath(this.Bounds.Size, false);
                context.AddPath(outlinePath);

                bool drawHighlighted = (this.Selected || this.Highlighted);
                CGColorSpace colorspace = CGColorSpace.CreateDeviceRGB();
                CGPoint endPoint = new CGPoint(0, this.Bounds.Size.Height);
                nfloat red = 1;
                nfloat green = 1;
                nfloat blue = 1;
                nfloat alpha = 1;
                this.GetTintColorRed(ref red, ref green, ref blue, ref alpha);

                if (drawHighlighted)
                {
                    context.SetFillColor(new nfloat[] { red, green, blue, 1 });
                    context.FillPath();
                }
                else
                {
                    context.Clip();
                    nfloat[] location = new nfloat[] { 0f, 0.95f };
                    nfloat[] components = new nfloat[] { red + 0.2f, green + 0.2f, blue + 0.2f, alpha, red, green, blue, 0.8f };
                    CGGradient gradients = new CGGradient(colorspace, components, location);
                    context.DrawLinearGradient(gradients, CGPoint.Empty, endPoint, 0);
                }

                context.RestoreState();

                CGPath innerPath = CreateTokenPath(this.Bounds.Size, true);

                // Draw a white background so we can use alpha to lighten the inner gradient
                context.SaveState();
                context.AddPath(innerPath);
                context.SetFillColor(new nfloat[] { 1f, 1f, 1f, 1f });
                context.FillPath();
                context.RestoreState();

                // Draw the inner gradient.
                context.SaveState();
                context.AddPath(innerPath);
                context.Clip();



                nfloat[] locations = new nfloat[] { 0f, (drawHighlighted ? 0.9f : 0.6f) };
                nfloat[] highlightedComp = new nfloat[] { red, green, blue, 0.7f, red, green, blue, 1f };
                nfloat[] nonHighlightedComp = new nfloat[] { red, green, blue, 0.15f, red, green, blue, 0.3f };

                CGGradient gradient = new CGGradient(colorspace, (drawHighlighted ? highlightedComp : nonHighlightedComp), locations);
                context.DrawLinearGradient(gradient, CGPoint.Empty, endPoint, 0);
                context.RestoreState();

                nfloat accessoryWidth = 0;
                nfloat ignore = 0;

                if (_accessoryType == AccessoryType.DisclosureIndicator)
                {
                    CGPoint arrowPoint = new CGPoint(this.Bounds.Size.Width - (float)Math.Floor(hTextPadding / 2), (this.Bounds.Size.Height / 2) - 1);
                    CGPath disclosurePath = this.CreateDisclosureIndicatorPath(arrowPoint, _font.PointSize, kDisclosureThickness, out accessoryWidth);
                    accessoryWidth += (float)Math.Floor(hTextPadding / 2);

                    context.AddPath(disclosurePath);
                    context.SetFillColor(new nfloat[] { 1, 1, 1, 1 });

                    if (drawHighlighted)
                    {
                        context.FillPath();
                    }
                    else
                    {
                        context.SaveState();
                        context.SetShadow(new CGSize(0, 1), 1, UIColor.White.ColorWithAlpha(0.6f).CGColor);
                        context.FillPath();
                        context.RestoreState();

                        context.SaveState();
                        context.AddPath(disclosurePath);
                        context.Clip();

                        CGGradient disclosureGradient = new CGGradient(colorspace, highlightedComp, null);
                        context.DrawLinearGradient(disclosureGradient, CGPoint.Empty, endPoint, 0);
                        arrowPoint.Y += 0.5f;
                        CGPath innerShadowPath = this.CreateDisclosureIndicatorPath(arrowPoint, _font.PointSize, kDisclosureThickness, out ignore);
                        context.AddPath(innerShadowPath);

                        context.SetStrokeColor(new nfloat[] { 0f, 0f, 0f, 0.3f });
                        context.StrokePath();
                        context.RestoreState();
                    }
                }

                NSString title = new NSString(this.Title);

                CGSize titleSize = title.StringSize(this.Font, (_maxWidth - hTextPadding - accessoryWidth), kLineBreakMode);
                nfloat vPadding = (nfloat)Math.Floor((this.Bounds.Size.Height - titleSize.Height) / 2f);
                nfloat titleWidth = (nfloat)Math.Ceiling(this.Bounds.Size.Width - hTextPadding - accessoryWidth);
                CGRect textBounds = new CGRect((float)Math.Floor(hTextPadding / 2), vPadding - 1, titleWidth, (float)Math.Floor(this.Bounds.Size.Height - (vPadding * 2)));

					context.SetFillColor((drawHighlighted ? this.HighlightedTextColor : this.TextColor).CGColor);

                title.DrawString(textBounds, this.Font, kLineBreakMode);
            });
        }

        public override string ToString()
        {
            return string.Format("[TIToken: Title={0}, RepresentedObject={1}]", Title, RepresentedObject);
        }
        #endregion

        #region Protected Methods

        protected virtual CGPath CreateTokenPath(CGSize size, bool innerPath) 
        {
            return Wrap.Function("CreateTokenPath", delegate()
            {
                return UIBezierPath.FromRoundedRect(new CGRect(0,0,size.Width, size.Height), this.TokenRadius).CGPath;
            });
        }
        protected virtual CGPath CreateDisclosureIndicatorPath(CGPoint arrowPointFront, nfloat height, nfloat thickness, out nfloat width)
        {
            nfloat out_width = 0; // fix for method wrapping
            CGPath result = Wrap.Function("CreateDisclosureIndicatorPath", delegate()
            {
                thickness /= (float)Math.Cos(Math.PI / 4);
                CGPath path = new CGPath(); //CGPathCreateMutable();
                path.MoveToPoint(arrowPointFront.X, arrowPointFront.Y);

                CGPoint bottomPointFront = new CGPoint(arrowPointFront.X - (float)(height / (2 * Math.Tan(Math.PI / 4))), arrowPointFront.Y - height / 2);
                path.AddLineToPoint(bottomPointFront.X, bottomPointFront.Y);


                CGPoint bottomPointBack = new CGPoint(bottomPointFront.X - thickness * (float)Math.Cos(Math.PI / 4), bottomPointFront.Y + thickness * (float)Math.Sin(Math.PI / 4));
                path.AddLineToPoint(bottomPointBack.X, bottomPointBack.Y);

                CGPoint arrowPointBack = new CGPoint(arrowPointFront.X - thickness / (float)Math.Cos(Math.PI / 4), arrowPointFront.Y);
                path.AddLineToPoint(arrowPointBack.X, arrowPointBack.Y);

                CGPoint topPointFront = new CGPoint(bottomPointFront.X, arrowPointFront.Y + height / 2);
                CGPoint topPointBack = new CGPoint(bottomPointBack.X, topPointFront.Y - thickness * (float)Math.Sin(Math.PI / 4));

                path.AddLineToPoint(topPointBack.X, topPointBack.Y);
                path.AddLineToPoint(topPointFront.X, topPointFront.Y);
                path.AddLineToPoint(arrowPointFront.X, arrowPointFront.Y);

                out_width = (arrowPointFront.X - topPointBack.X);
                return path;
            });
            width = out_width;
            return result;
        }
        protected virtual bool GetTintColorRed(ref nfloat red, ref nfloat green, ref nfloat blue, ref nfloat alpha)
        {
            CGColorSpaceModel colorSpaceModel = this.TintColor.CGColor.ColorSpace.Model;
            nfloat[] components = this.TintColor.CGColor.Components;

            if ((colorSpaceModel == CGColorSpaceModel.Monochrome) || (colorSpaceModel == CGColorSpaceModel.RGB))
            {
                red = components[0];
                green = (colorSpaceModel == CGColorSpaceModel.Monochrome ? components[0] : components[1]);
                blue = (colorSpaceModel == CGColorSpaceModel.Monochrome ? components[0] : components[2]);
                alpha = (colorSpaceModel == CGColorSpaceModel.Monochrome ? components[1] : components[3]);

                return true;
            }

            return false;
        }

        #endregion

    }
}

