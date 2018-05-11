using System;
using System.Windows;
using System.Windows.Controls;

namespace DependenciesVisualizer.UserControls
{
    /// <summary>
    /// Interaction logic for ClosablePanelMessage.xaml
    /// </summary>
    public partial class ClosablePanelMessage : UserControl
    {
        public ClosablePanelMessage()
        {
            InitializeComponent();
        }

        public String Message
        {
            get
            {
                return (String)GetValue(MessageProperty);
            }
            set
            {
                SetValue(MessageProperty, value);
            }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
                                                                                                "Message",
                                                                                                typeof(string),
                                                                                                typeof(ClosablePanelMessage),
                                                                                                new UIPropertyMetadata(null, 
                                                                                                    null,
                                                                                                    new CoerceValueCallback(CoerceMessageChanged)
                                                                                                    ));

        public static object CoerceMessageChanged(DependencyObject d, object value)
        {
            if (string.IsNullOrWhiteSpace((string)value))
            {
                ((ClosablePanelMessage)d).theGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                ((ClosablePanelMessage)d).theGrid.Visibility = Visibility.Visible;
            }

            return value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.theGrid.Visibility = Visibility.Collapsed;
        }
    }
}
