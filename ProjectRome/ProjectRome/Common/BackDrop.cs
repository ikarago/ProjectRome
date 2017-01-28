//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

// Based on the WindowsUIDevLabs Backdrop-control, together with "noise.jpg"
// Modified by Ikarago to suit the needs of the application


using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using SamplesCommon;
using System;
using Windows.Foundation;

namespace ProjectRome.Common
{
    public class BackDrop : Control
    {
        Compositor compositor;
        SpriteVisual blurVisual;
        CompositionBrush blurBrush;
        Visual rootVisual;

        bool setUpExpressions;
        CompositionSurfaceBrush noiseBrush;


        // Constructor
        public BackDrop()
        {
            rootVisual = ElementCompositionPreview.GetElementVisual(this as UIElement);
            Compositor = rootVisual.Compositor;

            blurVisual = Compositor.CreateSpriteVisual();
            noiseBrush = Compositor.CreateSurfaceBrush();

            CompositionEffectBrush brush = BuildBlurBrush();
            brush.SetSourceParameter("source", compositor.CreateHostBackdropBrush());
            blurBrush = brush;
            blurVisual.Brush = blurBrush;

            BlurAmount = 9;
            TintColor = Colors.Transparent;

            ElementCompositionPreview.SetElementChildVisual(this as UIElement, blurVisual);

            this.Loading += OnLoading;
            this.Unloaded += OnUnloaded;
        }

        private async void OnLoading(FrameworkElement sender, object args)
        {
            this.SizeChanged += OnSizeChanged;
            OnSizeChanged(this, null);

            SurfaceLoader.Initialize(compositor);
            noiseBrush.Surface = await SurfaceLoader.LoadFromUri(new Uri("ms-appx:///Assets/Temp/Noise.jpg"));
            noiseBrush.Stretch = CompositionStretch.UniformToFill;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        private void OnSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            if (blurVisual != null)
            {
                blurVisual.Size = new System.Numerics.Vector2((float)this.ActualWidth, (float)this.ActualHeight);
            }
        }


        // Public stuff
        public const string BlurAmountProperty = nameof(BlurAmount);
        public const string TintColorProperty = nameof(TintColor);

        public double BlurAmount
        {
            get
            {
                float value = 0;
                rootVisual.Properties.TryGetScalar(BlurAmountProperty, out value);
                return value;
            }
            set
            {
                if (!setUpExpressions)
                {
                    blurBrush.Properties.InsertScalar("Blur.BlurAmount", (float)value);
                }
                rootVisual.Properties.InsertScalar(BlurAmountProperty, (float)value);
            }
        }

        public Color TintColor
        {
            get
            {
                Color value;
                rootVisual.Properties.TryGetColor("TintColor", out value);
                //value = ((CompositionColorBrush)blurBrush).Color;
                return value;
            }
            set
            {
                if (!setUpExpressions)
                {
                    blurBrush.Properties.InsertColor("Color.Color", value);
                }
                rootVisual.Properties.InsertColor(TintColorProperty, value);
                //((CompositionColorBrush)blurBrush).Color = value;
            }
        }

        //public CompositionPropertySet VisualProperties
        //{
        //    get
        //    {
        //        if (!setUpExpressions)
        //        {
        //            SetUpPropertySetExpressions();
        //        }
        //        return rootVisual.Properties;
        //    }
        //}

        public Compositor Compositor
        {
            get
            {
                return compositor;
            }
            private set
            {
                compositor = value;
            }
        }


        // Methods 'n stuff
        //private void SetUpPropertySetExpressions()
        //{
        //    setUpExpressions = true;


        //}

        private CompositionEffectBrush BuildBlurBrush()
        {
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source"),
            };

            BlendEffect blendEffect = new BlendEffect
            {
                Background = blurEffect,
                Foreground = new ColorSourceEffect { Name = "Color", Color = Color.FromArgb(80,255,255,255)},
                Mode = BlendEffectMode.SoftLight
            };

            SaturationEffect saturationEffect = new SaturationEffect
            {
                Source = blendEffect,
                Saturation = 1.75f,
            };

            BlendEffect finalEffect = new BlendEffect
            {
                Foreground = new CompositionEffectSourceParameter("NoiseImage"),
                Background = saturationEffect,
                Mode = BlendEffectMode.Screen,
            };

            var factory = Compositor.CreateEffectFactory(finalEffect, new[] {"Blur.BlurAmount", "Color.Color" });

            CompositionEffectBrush brush = factory.CreateBrush();
            brush.SetSourceParameter("NoiseImage", noiseBrush);
            return brush;
        }
    }
}
