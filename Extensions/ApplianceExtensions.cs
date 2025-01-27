using KitchenData;

namespace KitchenGameplayInfo.Extensions
{
    internal static class ApplianceExtensions
    {
        public static bool HasProperty<T>(this Appliance appliance) where T : IApplianceProperty
        {
            return RequireProperty<T>(appliance, out _);
        }

        public static bool RequireProperty<T>(this Appliance appliance, out T prop) where T : IApplianceProperty
        {
            if (appliance?.Properties != default)
            {
                for (int i = 0; i < appliance.Properties.Count; i++)
                {
                    if (!(appliance.Properties[i] is T))
                        continue;

                    prop = (T)appliance.Properties[i];
                    return true;
                }
            }

            prop = default;
            return false;
        }
    }
}
