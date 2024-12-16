using ImGuiNET;
using Newtonsoft.Json;
using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Cache;
using ExileCore2.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RectangleF = ExileCore2.Shared.RectangleF;
using Vector2 = System.Numerics.Vector2;

namespace UniqueLootHelper
{

    public class CustomItemData
    {
        public Entity Entity;
        public bool IsCorrupted, IsIdentified;
        public Element Element;
        public Vector2 Location;
        public RectangleF ClientRect;
        public string ResourcePath = string.Empty;
        public CustomItemData(Entity entity, Element element, Vector2 location)
        {
            Entity = entity;
            Element = element;
            Location = location;

            if (entity.TryGetComponent<RenderItem>(out var renderItem))
            {
                ResourcePath = entity.GetComponent<RenderItem>().ResourcePath;
            }
            if (entity.TryGetComponent<Base>(out var @base))
            {
                IsCorrupted = @base.isCorrupted;
            }
            if (entity.TryGetComponent<Mods>(out var mods))
            {
                IsIdentified = mods.Identified;
            }

        }


    }
    public class UniqueItemSettings
    {
        public string ArtPath, Label;
        public bool LineDrawWorld, DrawLabelInBox, DrawLabelOutline,
                    LineDrawMap, DrawLabelName, DrawIsCorrupted;
        public UniqueItemSettings()
        {
            ArtPath = "";
            Label = "";
            LineDrawMap = false;
            DrawLabelOutline = true;
            DrawLabelName = true;
            DrawLabelInBox = true;
            DrawIsCorrupted = true;
        }
    }

    public class UniqueLootHelperCore : BaseSettingsPlugin<Settings>
    {
        public static Graphics _graphics;
        private UniqueItemSettings _tempUniqueItemSettings = new();
        private const string FILE_ART_NAME = "UniquesArtworks.json";
        private string PathArtFile => Path.Combine(ConfigDirectory, FILE_ART_NAME);

        private CachedValue<List<CustomItemData>> _groundItems;

        private Dictionary<string, UniqueItemSettings> _cashUniqueArtWork = [];

        public UniqueLootHelperCore()
        {
            _groundItems = new FrameCache<List<CustomItemData>>(CacheUtils.RememberLastValue(GetItemsOnGround, new List<CustomItemData>()));
        }
        public override bool Initialise()
        {
            Name = "UniqueLootHelper";
            _cashUniqueArtWork = GetUniqueArtFromFile();

            return base.Initialise();
        }
        private void CreateUniqueArtFile()
        {
            if (File.Exists(PathArtFile)) return;
            File.WriteAllText(PathArtFile, JsonConvert.SerializeObject(new Dictionary<string, UniqueItemSettings>(), Formatting.Indented));
            LogMessage("UniqueLootHelper: Created new file for unique art");
        }

        private Dictionary<string, UniqueItemSettings> GetUniqueArtFromFile()
        {

            if (!File.Exists(PathArtFile)) CreateUniqueArtFile();
            try
            {
                var uniqueArtItemList = JsonConvert.DeserializeObject<Dictionary<string, UniqueItemSettings>>(File.ReadAllText(PathArtFile));
                return uniqueArtItemList;
            }
            catch (Exception)
            {
                File.Move(PathArtFile, PathArtFile + ".bak");
                CreateUniqueArtFile();
                return [];
            }


        }
        public override void OnUnload()
        {
            SaveUniquesArtToFile();
            base.OnUnload();
        }
        public override void OnClose()
        {
            SaveUniquesArtToFile();
            base.OnClose();
        }
        private void SaveUniquesArtToFile()
        {
            if (!File.Exists(PathArtFile))
                CreateUniqueArtFile();
            File.WriteAllText(PathArtFile, JsonConvert.SerializeObject(_cashUniqueArtWork, Formatting.Indented));
            LogMessage("UniqueLootHelper: Saved unique art to file");
        }


        public override void DrawSettings()
        {
            if (ImGui.Button("Open Config Folder"))
            {
                Process.Start("explorer.exe", ConfigDirectory);
            }
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            base.DrawSettings();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Text("Add new unique to list");
            ImGui.InputText("Unique art path", ref _tempUniqueItemSettings.ArtPath, 1024, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.InputText("Unique label", ref _tempUniqueItemSettings.Label, 1024, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.Checkbox("Draw line on map", ref _tempUniqueItemSettings.LineDrawMap);
            ImGui.SameLine();
            ImGui.Checkbox("Draw line on world", ref _tempUniqueItemSettings.LineDrawWorld);
            ImGui.SameLine();
            ImGui.Checkbox("Draw outline", ref _tempUniqueItemSettings.DrawLabelOutline);
            ImGui.SameLine();
            ImGui.Checkbox("Draw real name", ref _tempUniqueItemSettings.DrawLabelName);
            ImGui.SameLine();
            ImGui.Checkbox("Draw label in box", ref _tempUniqueItemSettings.DrawLabelInBox);
            ImGui.SameLine();

            ImGui.Checkbox("Draw is corrupted", ref _tempUniqueItemSettings.DrawIsCorrupted);
            if (ImGui.Button("Add Unique"))
            {
                if (!string.IsNullOrEmpty(_tempUniqueItemSettings.ArtPath) && !string.IsNullOrEmpty(_tempUniqueItemSettings.Label))
                {
                    string key = _tempUniqueItemSettings.ArtPath;
                    if (_cashUniqueArtWork.TryGetValue(key, out _))
                    {
                        // Key exists, update the value
                        _cashUniqueArtWork[key] = _tempUniqueItemSettings;
                        LogMessage($"UniqueLootHelper: Updated {key} in unique list");
                    }
                    else
                    {
                        // Key does not exist, add new key-value pair
                        _cashUniqueArtWork.Add(key, _tempUniqueItemSettings);
                        LogMessage($"UniqueLootHelper: Added {key} to unique list");
                    }

                    _tempUniqueItemSettings = new();

                }
            }
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Text("Uniques list:");
            foreach (var uniqueArtItem in _cashUniqueArtWork)
            {
                ImGui.Text($"{uniqueArtItem.Key} - {uniqueArtItem.Value.Label}");
                ImGui.SameLine();
                if (ImGui.Button($"Edit##{uniqueArtItem.Key}"))
                {
                    _tempUniqueItemSettings = uniqueArtItem.Value;

                    LogMessage($"UniqueLootHelper: Removed {uniqueArtItem.Key} from unique list");

                }
                ImGui.SameLine();
                if (ImGui.Button($"Delete##{uniqueArtItem.Key}"))
                {
                    _cashUniqueArtWork.Remove(uniqueArtItem.Key);

                    LogMessage($"UniqueLootHelper: Removed {uniqueArtItem.Key} from unique list");
                }
            }


        }


        public override void Render()
        {
            if (Input.IsKeyDown(Keys.F7))
            {

                var hoverItem = GameController.Game.IngameState.UIHover.AsObject<HoverItemIcon>();
                if (hoverItem == null) return;
                var renderItem = hoverItem.Item.GetComponent<RenderItem>();
                if (renderItem == null) return;
                ImGui.SetClipboardText(renderItem.ResourcePath);
                LogMessage($"UniqueLootHelper: Copied {renderItem.ResourcePath} to clipboard");

            }

            var inGameUi = GameController.Game.IngameState.IngameUi;
            if (!Settings.IgnoreFullscreenPanels && inGameUi.FullscreenPanels.Any(x => x.IsVisible))
            {
                return;
            }

            if (!Settings.IgnoreRightPanels && inGameUi.OpenRightPanel.IsVisible)
            {
                return;
            }

            Entity player = GameController?.Player;
            ImGui.Begin("lmao",
             ImGuiWindowFlags.NoDecoration
           | ImGuiWindowFlags.NoBackground
           | ImGuiWindowFlags.NoInputs
           | ImGuiWindowFlags.NoFocusOnAppearing
           | ImGuiWindowFlags.NoNav);
            var drawList = ImGui.GetBackgroundDrawList();
            var countList = new List<string>();
            foreach (var item in _groundItems.Value)
            {
                string[] pathArray = [item.ResourcePath, item.ResourcePath + ".dds", item.ResourcePath.Replace(".dds", "")];

                if (!pathArray.Any(_cashUniqueArtWork.ContainsKey))
                    continue;

                var uniqueSettings = _cashUniqueArtWork[pathArray.First(_cashUniqueArtWork.ContainsKey)];

                if (!uniqueSettings.DrawIsCorrupted && item.IsCorrupted)
                    continue;

                if (uniqueSettings.DrawLabelInBox)
                    countList.Add(uniqueSettings.Label);

                if (uniqueSettings.LineDrawMap && Settings.EnableMapDrawing && GameController.IngameState.IngameUi.Map.LargeMap.IsVisible)
                {
                    var itemMapPost = GameController.IngameState.Data.GetGridMapScreenPosition(item.Location);
                    var playerMapPost = GameController.IngameState.Data.GetGridMapScreenPosition(player.GridPos);
                    Graphics.DrawLine(
                    itemMapPost,
                    playerMapPost,
                    Settings.MapLineThickness,
                    Settings.MapLineColor
                );
                }
                if (Settings.WorldMapDrawing && uniqueSettings.LineDrawWorld)
                {
                    var itemWorldPos = GameController.IngameState.Data.GetGridScreenPosition(item.Location);
                    Vector2 playerWorldPos = GameController.IngameState.Data.GetGridScreenPosition(player.GridPos);
                    Graphics.DrawLine(
                           playerWorldPos,
                           itemWorldPos,
                            Settings.WorldMapLineThickness,
                            Settings.WorldMapLineColor);
                }

                var labelFrame = item.Element.GetClientRect();
                if (Settings.EnableOutlineLebel && uniqueSettings.DrawLabelOutline)
                {

                    Graphics.DrawFrame(labelFrame, Settings.OutlineLabelColor, Settings.LabelFrameThickness);
                }
                if (Settings.EnableLabelName && uniqueSettings.DrawLabelName && !item.IsIdentified)
                {
                    string text = uniqueSettings.Label;
                    var textSize = Graphics.MeasureText(text);
                    float scale = Math.Min(labelFrame.Width / textSize.X, (labelFrame.Height - 2) / textSize.Y) - 0.2f;
                    ImGui.SetWindowFontScale(scale);
                    var newTextSize = ImGui.CalcTextSize(text);
                    var textPosition = labelFrame.Center - newTextSize / 2;
                    var rectPosition = new Vector2(textPosition.X, labelFrame.Top + 1);
                    drawList.AddRectFilled(labelFrame.TopLeft, labelFrame.BottomRight, Settings.BackgroundLabel.Value.ToImgui());
                    drawList.AddText(textPosition, Settings.LabelTextColor.Value.ToImgui(), text);
                    ImGui.SetWindowFontScale(1);
                }
            }

            ImGui.End();

            if (Settings.EnableBoxCountDrawing)
                DrawItemCountInfo(countList);
        }


        private void DrawItemCountInfo(List<string> countList)
        {
            if (countList.Count == 0) return;
            var labelCount = countList.GroupBy(x => x).ToDictionary(group => group.Key, group => group.Count());
            var posX = Settings.BoxPositionX.Value;
            var posY = Settings.BoxPositionY.Value;
            var hight = labelCount.Count * 20 + 20;
            var rect = new RectangleF(posX, posY, 230, hight);
            Graphics.DrawBox(rect, Settings.BoxBackgroundColor);
            if (Settings.BoxOutline.Value == true)
                Graphics.DrawFrame(rect, Settings.BoxOutlineColor, 2);

            posX += 10;
            posY += 10;


            foreach (var item in labelCount)
            {
                Graphics.DrawText($"{item.Key}: {item.Value}", new Vector2(posX, posY), Settings.BoxTextColor);
                posY += 20;
            }
        }

        private List<CustomItemData> GetItemsOnGround(List<CustomItemData> previousValue)
        {
            var prevDict = previousValue
                .DistinctBy(x => (x.Entity?.Address, x.Element?.Address))
                .ToDictionary(x => (x.Element?.Address, x.Entity?.Address));
            var labelsOnGround = GameController.IngameState.IngameUi.ItemsOnGroundLabelElement.VisibleGroundItemLabels;
            var result = new List<CustomItemData>();

            foreach (var description in labelsOnGround)
            {
                if (description.Entity.TryGetComponent<WorldItem>(out var worldItem) &&
                    worldItem.ItemEntity is { IsValid: true } groundItemEntity)
                {

                    var customItem = prevDict.GetValueOrDefault((description.Label?.Address, groundItemEntity.Address));

                    if (customItem == null)
                    {
                        customItem = new CustomItemData(groundItemEntity, description.Label, description.Entity.GridPos);
                    }

                    result.Add(customItem);
                }
            }


            return result;
        }
    }

}
