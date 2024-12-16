using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using System.Windows.Forms;
using System.Drawing;

namespace UniqueLootHelper
{
    public class Settings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
        public HotkeyNode CopyToClipboardHoverItemPath { get; set; } = new HotkeyNode(Keys.F7);
        public ToggleNode IgnoreFullscreenPanels { get; set; } = new(false);
        public ToggleNode IgnoreRightPanels { get; set; } = new(false);
        public RangeNode<int> LabelFrameThickness { get; set; } = new RangeNode<int>(2, 1, 10);
        public ToggleNode EnableBoxCountDrawing { get; set; } = new(true);
        public ColorNode BoxBackgroundColor { get; set; } = new ColorNode(Color.Black);
        public ColorNode BoxOutlineColor { get; set; } = new ColorNode(Color.Red);
        public ColorNode BoxTextColor { get; set; } = new ColorNode(Color.Red);
        public ToggleNode BoxOutline { get; set; } = new ToggleNode(false);
        public RangeNode<float> BoxPositionX { get; set; } = new RangeNode<float>(576.0f, 0f, 3000f);
        public RangeNode<float> BoxPositionY { get; set; } = new RangeNode<float>(576.0f, 0f, 3000f);

        public ToggleNode EnableOutlineLebel { get; set; } = new(true);
        public ColorNode OutlineLabelColor { get; set; } = new ColorNode(Color.Red);
        public ToggleNode EnableLabelName { get; set; } = new(true);
        public ColorNode BackgroundLabel { get; set; } = new(Color.White);
        public ColorNode LabelTextColor { get; set; } = new(Color.Red);
        public ToggleNode EnableMapDrawing { get; set; } = new(true);
        public ColorNode MapLineColor
        { get; set; } = new(Color.Purple);
        public RangeNode<int> MapLineThickness { get; set; } = new(2, 1, 10);
        public ToggleNode WorldMapDrawing { get; set; } = new(true);
        public ColorNode WorldMapLineColor { get; set; } = new(Color.Purple);
        public RangeNode<int> WorldMapLineThickness { get; set; } = new(2, 1, 10);
        public ToggleNode UseCorruptedFilter { get; set; } = new(false);

    }
}
