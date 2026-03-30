namespace MyPrint
{
    public enum ColorConsole
    {
        Red,
        Blue,
        Green,
        Yellow,
        Orange,
        Pink,
        Purple,
        Black,
        Grey,
        White
    }

    public enum ConsoleStyle
    {
        Bold,
        Italic,
        Underline,
    }

    public class ConsoleOption
    {
        public static string GetColor(ColorConsole color)
        {
            switch (color)
            {
                case ColorConsole.Red: return "<color=red>";
                case ColorConsole.Blue: return "<color=blue>";
                case ColorConsole.Green: return "<color=green>";
                case ColorConsole.Yellow: return "<color=yellow>";
                case ColorConsole.Orange: return "<color=orange>";
                case ColorConsole.Pink: return "<color=#FFC0CB>";
                case ColorConsole.Purple: return "<color=purple>";
                case ColorConsole.Black: return "<color=black>";
                case ColorConsole.Grey: return "<color=grey>";
                case ColorConsole.White:
                default: return "<color=white>";
            }
        }

        public static (string, string) GetStyle(ConsoleStyle style)
        {
            switch (style)
            {
                case ConsoleStyle.Bold: return ("<b>", "</b>");
                case ConsoleStyle.Italic: return ("<i>", "</i>");
                default: return ("", "");
            }
        }
    }
}
