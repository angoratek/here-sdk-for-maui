#if IOS
using UIKit;

namespace Here.Explore.Maui.RefApp;

public class Program
{
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
#endif
