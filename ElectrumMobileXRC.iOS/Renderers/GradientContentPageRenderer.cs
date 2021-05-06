using System;
using CoreAnimation;
using CoreGraphics;
using ElectrumMobileXRC.Controls;
using ElectrumMobileXRC.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(GradientContentPage), typeof(GradientContentPageRenderer))]
namespace ElectrumMobileXRC.iOS.Renderers
{
    public class GradientContentPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null) // perform initial setup
            {
                var page = e.NewElement as GradientContentPage;

                var gradientLayer = new CAGradientLayer
                {
                    Frame = View.Bounds,
                    Colors = new CGColor[] { page.StartColor.ToCGColor(), page.EndColor.ToCGColor() }
                };

                View.Layer.InsertSublayer(gradientLayer, 0);
            }
        }
    }
}
