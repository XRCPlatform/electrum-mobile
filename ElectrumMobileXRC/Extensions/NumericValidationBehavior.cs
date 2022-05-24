using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace ElectrumMobileXRC.Extensions
{
    public class NumericValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            entry.TextChanged += OnEntryTextChanged;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            base.OnDetachingFrom(entry);
            entry.TextChanged -= OnEntryTextChanged;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.OldTextValue == e.NewTextValue)
            {
                //do nothing
            }
            else
            {
                var entry = (Entry)sender;

                if (e.NewTextValue.Contains(",")) {
                    if (!e.NewTextValue.Contains("."))
                    {
                        entry.Text = e.NewTextValue.Replace(",", ".");
                    }
                    else
                    {
                        entry.Text = e.OldTextValue;
                    }
                }
            }
        }
    }
}
