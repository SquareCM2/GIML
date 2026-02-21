using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace GIML
{
    public static class RichTextHelper
    {
        // 定义附加属性 FormattedText
        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(string),
                typeof(RichTextHelper),
                new PropertyMetadata(null, OnFormattedTextChanged));

        public static void SetFormattedText(RichTextBlock element, string value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(RichTextBlock element)
        {
            return (string)element.GetValue(FormattedTextProperty);
        }

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBlock richTextBlock && e.NewValue is string text)
            {
                // 调用你已有的转换器生成 Paragraph
                var paragraph = MindustryTextConverter.ToRichTextParagraph(text);
                richTextBlock.Blocks.Clear();
                if (paragraph != null)
                {
                    richTextBlock.Blocks.Add(paragraph);
                }
            }
        }
    }
}