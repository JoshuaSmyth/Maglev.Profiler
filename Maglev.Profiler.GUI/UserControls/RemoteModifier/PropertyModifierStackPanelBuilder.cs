using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MaglevProfiler;

namespace TinyFrog.Profiler.Gui.UserControls.RemoteModifier
{
    public class PropertyModifierStackPanelBuilder
    {
        private StackPanel CreateContainerPanel(UIElement a, UIElement b)
        {
            var sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2)
            };
            sp.Children.Add(a);
            sp.Children.Add(b);
            return sp;
        }

        public StackPanel BuildEnumStackPanel(RemotelyModifiablePropertyInfo property, Action<RemotelyModifiablePropertyInfo> callback)
        {
            var cmb = new ComboBox();
            cmb.Items.Clear();
            foreach (var value in property.PotentialValues)
            {
                cmb.Items.Add(value);
            }

            cmb.SelectedItem = property.PropertyValue;

            cmb.SelectionChanged += (o, e) =>
            {
                property.PropertyValue = cmb.SelectedItem.ToString();
                callback.Invoke(property);
            };

            var tb = new TextBlock
            {
                Text = property.PropertyName,
                Margin = new Thickness(2)
            };

            return CreateContainerPanel(cmb, tb);
        }

        public StackPanel BuildBooleanStackPanel(RemotelyModifiablePropertyInfo property, Action<RemotelyModifiablePropertyInfo> callback)
        {
            var cb = new CheckBox()
                {
                    Margin = new Thickness(2,2,100,2),
                    IsChecked = Boolean.Parse(property.PropertyValue),
                };

            cb.Checked += (o, e) =>
                {
                    property.PropertyValue = "True";
                    callback.Invoke(property);
                };

            cb.Unchecked += (o, e) =>
                {
                    property.PropertyValue = "False";
                    callback.Invoke(property);
                };

                var tb = new TextBlock 
                {
                    Text = property.PropertyName,
                    Margin = new Thickness(2)
                };

            return CreateContainerPanel(cb, tb);
        }

        public StackPanel BuildSliderStackPanel(RemotelyModifiablePropertyInfo property, Action<RemotelyModifiablePropertyInfo> callback)
        {
            var cb = new Slider
                {
                    Margin = new Thickness(2, 2, 20, 2),
                    Width = 120,
                    Value = Double.Parse(property.PropertyValue),
                };

            cb.ValueChanged += (o, e) =>
            {
                property.PropertyValue = cb.Value.ToString();
                callback.Invoke(property);
            };

            var tb = new TextBlock
            {
                Text = property.PropertyName,
                Margin = new Thickness(2)
            };

            return CreateContainerPanel(cb,tb);
        }


        public StackPanel BuildIntegerStackPanel(RemotelyModifiablePropertyInfo property, Action<RemotelyModifiablePropertyInfo> callback)
        {
            var cb = new TextBox
                {
                    Margin = new Thickness(2, 2, 20, 2),
                    Width = 80,
                    Text = property.PropertyValue,
                };

            cb.TextChanged += (o, e) =>
                {
                    if (cb.Text == "")
                        return;

                    property.PropertyValue = cb.Text;
                    callback.Invoke(property);
                };


            var tb = new TextBlock
            {
                Text = property.PropertyName,
                Margin = new Thickness(2)
            };

            return CreateContainerPanel(cb, tb);
        }

        public UIElement BuildNotStaticStackPanel(RemotelyModifiablePropertyInfo property, Action<RemotelyModifiablePropertyInfo> sendModifyPropertyMessage)
        {
            var tb = new TextBlock
            {
                Text = property.PropertyName,
                Margin = new Thickness(2)
            };

            var tbError = new TextBlock
            {
                Text = "Property Must be static to modifiy",
                Margin = new Thickness(2)
            };

            return CreateContainerPanel(tbError, tb);
        }
    }
}
