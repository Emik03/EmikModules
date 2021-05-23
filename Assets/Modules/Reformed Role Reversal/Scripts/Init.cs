/// <summary>
/// The center core of the module that calls other methods and classes.
/// </summary>
namespace ReformedRoleReversalModule
{
    internal class Init
    {
        internal Init(ReformedRoleReversalCoroutineHandler coroutines, ReformedRoleReversal reversal)
        {
            Reversal = reversal;
            Coroutines = coroutines;

            Interact = new Interact(this);
            Manual = new HandleManual(coroutines, this);
        }

        /// <summary>
        /// First dimension represents the tutorial and 3 through 9 wires, while second dimension correspond through conditions 1 through 8.
        /// </summary>
        protected internal readonly Condition[,] Conditions = new Condition[8, 8];

        protected internal readonly ReformedRoleReversalCoroutineHandler Coroutines;
        protected internal readonly HandleManual Manual;
        protected internal readonly Interact Interact;
        protected internal readonly ReformedRoleReversal Reversal;

        protected internal bool Solved, Ready;
        protected internal static int ModuleIdCounter;
        protected internal int ModuleId;

        /// <summary>
        /// Initalizes the module.
        /// </summary>
        protected internal void Activate()
        {
            ModuleId = ++ModuleIdCounter;

            SFX.LogVersionNumber(Reversal.Module, ModuleId);

            Manual.Generate();

            Reversal.Screen.OnInteract += delegate ()
            {
                Interact.PressScreen();
                return false;
            };

            for (byte i = 0; i < Reversal.Buttons.Length; i++)
            {
                byte j = i;
                Reversal.Buttons[i].OnInteract += delegate ()
                {
                    Interact.PressButton(ref j);
                    return false;
                };
            }
        }
    }
}
