namespace UHFPS.Input
{
    /// <summary>
    /// Class that contains all input constants.
    /// </summary>
    public sealed class Controls
    {
        // player movement
        public const string MOVEMENT = "input.action.movement";
        public const string SPRINT = "input.action.sprint";
        public const string JUMP = "input.action.jump";
        public const string CROUCH = "input.action.crouch";
        public const string LOOK = "input.action.look*";

        // player
        public const string USE = "input.action.use";
        public const string EXAMINE = "input.action.examine";
        public const string FIRE = "input.action.fire";
        public const string RELOAD = "input.action.reload";
        public const string ADS = "input.action.ads";
        public const string LEAN = "input.action.lean";
        public const string FLASHLIGHT = "input.action.flashlight";

        // ui
        public const string PAUSE = "input.action.pause";
        public const string MAP = "input.action.map";
        public const string INVENTORY = "input.action.inventory";
        public const string INVENTORY_ITEM_MOVE = "input.action.inventory.move";
        public const string INVENTORY_ITEM_ROTATE = "input.action.inventory.rotate";
        public const string SHOW_CURSOR = "input.action.cursor";
        public const string SHORTCUT_PREFIX = "input.action.equip";
        public const string ITEM_UNEQUIP = "input.action.unequip";
        public const string AXIS_ARROWS = "input.action.arrows";

        // other
        public const string POINTER = "input.action.mouse.point";
        public const string POINTER_DELTA = "input.action.mouse.delta";
        public const string SCROLL_WHEEL = "input.action.mouse.scrollwheel";
        public const string LEFT_BUTTON = "input.action.leftbutton";
        public const string RIGHT_BUTTON = "input.action.rightbutton";
    }
}