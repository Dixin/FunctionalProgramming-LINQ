namespace Tutorial.Tests.Android
{
    using System.Reflection;
    using global::Android.App;
    using global::Android.OS;
    using Xamarin.Android.NUnitLite;

    [Activity(Label = "Tutorial.Tests.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : TestSuiteActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            // tests can be inside the main assembly
            this.AddTest(Assembly.GetExecutingAssembly());
            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            base.OnCreate(bundle);
        }
    }
}

