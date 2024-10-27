using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InstrumentStatusDashboard.Controls
{
    [TemplatePart(Name = TextBlockValidation.ElementTextBlockHeader, Type = typeof(TextBlock))]
    [TemplatePart(Name = TextBlockValidation.ElementTextBlockErrorMessage, Type = typeof(TextBlock))]
    public class TextBlockValidation : Control
    {
        private const string ElementTextBlockHeader = "PART_TextBlockHeader";
        private const string ElementTextBlockErrorMessage = "PART_TextBlockErrorMessage";

        private TextBlock textBlockErrorMessage;

        static TextBlockValidation()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBlockValidation), new FrameworkPropertyMetadata(typeof(TextBlockValidation)));
        }

        public TextBlockValidation()
        {
            textBlockErrorMessage = GetTemplateChild(ElementTextBlockErrorMessage) as TextBlock;
        }

        public bool ShowHeader
        {
            get
            {
                return (bool)GetValue(ShowHeaderProperty);
            }
            set
            {
                SetValue(ShowHeaderProperty, value);
            }
        }

        public static readonly DependencyProperty ShowHeaderProperty =
            DependencyProperty.Register("ShowHeader", typeof(bool), typeof(TextBlockValidation));

        public string Header
        {
            get
            {
                return (string)GetValue(HeaderProperty);
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(TextBlockValidation));

        public string ErrorMessage
        {
            get
            {
                return (string)GetValue(ErrorMessageProperty);
            }
            set
            {
                SetValue(ErrorMessageProperty, value);
            }
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(TextBlockValidation));

        public bool HasError
        {
            get
            {
                return (bool)GetValue(HasErrorProperty);
            }
            set
            {
                SetValue(HasErrorProperty, value);
            }
        }

        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register("HasError", typeof(bool), typeof(TextBlockValidation));
    }
}
