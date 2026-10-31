using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Self-contained local workshop: browse prototype parts, equip one item per
/// slot, inspect derived stats, and save/load the build in user://.
/// </summary>
public partial class WorkshopScreen : Control
{
    private const int MaxWeight = 100;
    private const int MaxBuildCost = 50;

    private HBoxContainer _wideBody;
    private ScrollContainer _compactScroll;
    private VBoxContainer _compactBody;
    private PanelContainer _cataloguePanel;
    private PanelContainer _previewPanel;
    private PanelContainer _detailsPanel;
    private MarginContainer _pageMargin;
    private Label _offlineBadge;
    private bool _isCompactLayout;

    private readonly Dictionary<MechPartSlot, WorkshopPartDefinition> _loadout = new();
    private OptionButton _filter;
    private VBoxContainer _partList;
    private Label _selectedName;
    private Label _selectedDescription;
    private Label _selectedStats;
    private Label _loadoutSummary;
    private Label _status;
    private Label _buildStatus;
    private Label _healthValue;
    private Label _weightValue;
    private Label _costValue;
    private Label _energyValue;
    private Label _coolingValue;
    private Label _armorValue;
    private Button _equipButton;
    private Button _removeButton;
    private MechWorkshopPreview _preview;
    private WorkshopPartDefinition _selectedPart;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        BuildInterface();
        GetViewport().SizeChanged += OnViewportSizeChanged;
        LoadLocalBuild();
        RefreshAll();
        CallDeferred(nameof(UpdateResponsiveLayout));
    }

    private void BuildInterface()
    {
        var background = new ColorRect
        {
            Color = new Color(0.035f, 0.05f, 0.065f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(background);

        _pageMargin = new MarginContainer();
        _pageMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _pageMargin.AddThemeConstantOverride("margin_left", 22);
        _pageMargin.AddThemeConstantOverride("margin_right", 22);
        _pageMargin.AddThemeConstantOverride("margin_top", 18);
        _pageMargin.AddThemeConstantOverride("margin_bottom", 18);
        AddChild(_pageMargin);

        var page = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        page.AddThemeConstantOverride("separation", 14);
        _pageMargin.AddChild(page);

        BuildHeader(page);

        var bodyHost = new Control
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        page.AddChild(bodyHost);

        _wideBody = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _wideBody.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _wideBody.AddThemeConstantOverride("separation", 14);
        bodyHost.AddChild(_wideBody);

        _compactScroll = new ScrollContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            Visible = false
        };
        _compactScroll.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        bodyHost.AddChild(_compactScroll);

        _compactBody = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _compactBody.AddThemeConstantOverride("separation", 12);
        _compactScroll.AddChild(_compactBody);

        BuildCataloguePanel(_wideBody);
        BuildPreviewPanel(_wideBody);
        BuildDetailsPanel(_wideBody);

        var footer = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        footer.AddThemeConstantOverride("separation", 10);
        page.AddChild(footer);

        _status = NewLabel("New local build created.", 13, new Color(0.62f, 0.76f, 0.80f));
        _status.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        footer.AddChild(_status);

        var reset = MakeButton("RESET BUILD", false);
        reset.Pressed += ResetBuild;
        footer.AddChild(reset);

        var save = MakeButton("SAVE BUILD", true);
        save.Pressed += SaveBuild;
        footer.AddChild(save);
    }

    private void BuildHeader(VBoxContainer parent)
    {
        var header = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        header.AddThemeConstantOverride("separation", 12);
        parent.AddChild(header);

        var titleBlock = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        header.AddChild(titleBlock);

        titleBlock.AddChild(NewLabel("SUPER MECH / WORKSHOP", 24, new Color(0.90f, 0.94f, 0.95f), true));
        titleBlock.AddChild(NewLabel("CONFIGURE YOUR FRAME  •  LOCAL PROTOTYPE BUILD", 11, new Color(0.43f, 0.62f, 0.68f)));

        _offlineBadge = NewLabel("OFFLINE WORKSHOP", 11, new Color(0.55f, 0.82f, 0.68f), true);
        _offlineBadge.HorizontalAlignment = HorizontalAlignment.Right;
        header.AddChild(_offlineBadge);
    }

    private void BuildCataloguePanel(HBoxContainer parent)
    {
        var panel = MakePanel();
        _cataloguePanel = panel;
        panel.CustomMinimumSize = new Vector2(265, 0);
        panel.SizeFlagsVertical = SizeFlags.ExpandFill;
        parent.AddChild(panel);

        var column = PanelColumn(panel);
        column.AddChild(NewLabel("PART CATALOGUE", 16, Colors.White, true));

        _filter = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _filter.AddItem("All components");
        _filter.AddItem("Frames");
        _filter.AddItem("Mobility");
        _filter.AddItem("Core modules");
        _filter.AddItem("Utility modules");
        _filter.ItemSelected += OnFilterChanged;
        column.AddChild(_filter);

        _partList = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _partList.AddThemeConstantOverride("separation", 8);

        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        scroll.AddChild(_partList);
        column.AddChild(scroll);

        column.AddChild(NewLabel("Prototype parts and balance values are original to this project.", 10,
            new Color(0.48f, 0.58f, 0.62f)));
    }

    private void BuildPreviewPanel(HBoxContainer parent)
    {
        var panel = MakePanel();
        _previewPanel = panel;
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        panel.SizeFlagsVertical = SizeFlags.ExpandFill;
        parent.AddChild(panel);

        var column = PanelColumn(panel);
        var title = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        title.AddChild(NewLabel("ASSEMBLY PREVIEW", 16, Colors.White, true));
        var spacer = new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        title.AddChild(spacer);
        title.AddChild(NewLabel("VECTOR PROTOTYPE", 10, new Color(0.42f, 0.65f, 0.69f)));
        column.AddChild(title);

        var center = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        column.AddChild(center);

        _preview = new MechWorkshopPreview
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(360, 340)
        };
        center.AddChild(_preview);

        _loadoutSummary = NewLabel("", 11, new Color(0.59f, 0.70f, 0.74f));
        _loadoutSummary.HorizontalAlignment = HorizontalAlignment.Center;
        _loadoutSummary.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        column.AddChild(_loadoutSummary);
    }

    private void BuildDetailsPanel(HBoxContainer parent)
    {
        var panel = MakePanel();
        _detailsPanel = panel;
        panel.CustomMinimumSize = new Vector2(285, 0);
        panel.SizeFlagsVertical = SizeFlags.ExpandFill;
        parent.AddChild(panel);

        var column = PanelColumn(panel);
        column.AddChild(NewLabel("SELECTED COMPONENT", 15, Colors.White, true));

        _selectedName = NewLabel("Choose a component", 18, new Color(0.88f, 0.93f, 0.94f), true);
        column.AddChild(_selectedName);

        _selectedDescription = NewLabel("Select an item from the catalogue to inspect its details.", 12,
            new Color(0.61f, 0.70f, 0.73f));
        _selectedDescription.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        column.AddChild(_selectedDescription);

        _selectedStats = NewLabel("", 12, new Color(0.73f, 0.83f, 0.85f));
        column.AddChild(_selectedStats);

        _equipButton = MakeButton("EQUIP COMPONENT", true);
        _equipButton.Disabled = true;
        _equipButton.Pressed += EquipSelectedPart;
        column.AddChild(_equipButton);

        _removeButton = MakeButton("REMOVE OPTIONAL MODULE", false);
        _removeButton.Pressed += RemoveOptionalPart;
        column.AddChild(_removeButton);

        AddDivider(column);
        column.AddChild(NewLabel("BUILD SUMMARY", 15, Colors.White, true));

        _healthValue = AddStatLine(column, "FRAME HEALTH");
        _weightValue = AddStatLine(column, "TOTAL WEIGHT");
        _costValue = AddStatLine(column, "BUILD COST");
        _energyValue = AddStatLine(column, "ENERGY RESERVE");
        _coolingValue = AddStatLine(column, "COOLING");
        _armorValue = AddStatLine(column, "ARMOR");

        _buildStatus = NewLabel("", 11, new Color(0.66f, 0.81f, 0.69f));
        _buildStatus.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        column.AddChild(_buildStatus);

        column.AddChild(NewLabel("One component per slot. Frame and mobility are required.", 10,
            new Color(0.48f, 0.58f, 0.62f)));
    }

    private void OnViewportSizeChanged()
    {
        UpdateResponsiveLayout();
    }

    private void UpdateResponsiveLayout()
    {
        float viewportWidth = GetViewportRect().Size.X;
        bool useCompactLayout = WorkshopLayoutRules.UseCompactLayout(viewportWidth);
        if (_wideBody == null || _compactBody == null || useCompactLayout == _isCompactLayout)
            return;

        _isCompactLayout = useCompactLayout;

        if (_isCompactLayout)
        {
            MovePanel(_cataloguePanel, _compactBody);
            MovePanel(_previewPanel, _compactBody);
            MovePanel(_detailsPanel, _compactBody);

            foreach (PanelContainer panel in new[] { _cataloguePanel, _previewPanel, _detailsPanel })
            {
                panel.CustomMinimumSize = Vector2.Zero;
                panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                panel.SizeFlagsVertical = SizeFlags.ShrinkBegin;
            }

            _preview.CustomMinimumSize = new Vector2(0, 280);
            _wideBody.Visible = false;
            _compactScroll.Visible = true;
            int compactMargin = WorkshopLayoutRules.HorizontalMargin(viewportWidth);
            _pageMargin.AddThemeConstantOverride("margin_left", compactMargin);
            _pageMargin.AddThemeConstantOverride("margin_right", compactMargin);
            _pageMargin.AddThemeConstantOverride("margin_top", 10);
            _pageMargin.AddThemeConstantOverride("margin_bottom", 10);
            _offlineBadge.Visible = viewportWidth >= 600;
        }
        else
        {
            MovePanel(_cataloguePanel, _wideBody);
            MovePanel(_previewPanel, _wideBody);
            MovePanel(_detailsPanel, _wideBody);

            _cataloguePanel.CustomMinimumSize = new Vector2(265, 0);
            _cataloguePanel.SizeFlagsHorizontal = SizeFlags.Fill;
            _cataloguePanel.SizeFlagsVertical = SizeFlags.ExpandFill;
            _previewPanel.CustomMinimumSize = Vector2.Zero;
            _previewPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _previewPanel.SizeFlagsVertical = SizeFlags.ExpandFill;
            _detailsPanel.CustomMinimumSize = new Vector2(285, 0);
            _detailsPanel.SizeFlagsHorizontal = SizeFlags.Fill;
            _detailsPanel.SizeFlagsVertical = SizeFlags.ExpandFill;

            _preview.CustomMinimumSize = new Vector2(360, 340);
            _wideBody.Visible = true;
            _compactScroll.Visible = false;
            _pageMargin.AddThemeConstantOverride("margin_left", 22);
            _pageMargin.AddThemeConstantOverride("margin_right", 22);
            _pageMargin.AddThemeConstantOverride("margin_top", 18);
            _pageMargin.AddThemeConstantOverride("margin_bottom", 18);
            _offlineBadge.Visible = true;
        }
    }

    private static void MovePanel(PanelContainer panel, Container target)
    {
        if (panel == null || target == null || panel.GetParent() == target)
            return;

        if (panel.GetParent() is Container currentParent)
            currentParent.RemoveChild(panel);

        target.AddChild(panel);
    }

    public override void _ExitTree()
    {
        if (GetViewport() != null)
            GetViewport().SizeChanged -= OnViewportSizeChanged;
    }

    private void LoadLocalBuild()
    {
        bool loaded = WorkshopSaveStore.TryLoad(out WorkshopLoadoutData data, out string message);
        _loadout.Clear();

        foreach (KeyValuePair<string, string> entry in data.EquippedPartIds)
        {
            if (!Enum.TryParse(entry.Key, true, out MechPartSlot slot) ||
                !Enum.IsDefined(typeof(MechPartSlot), slot))
                continue;

            WorkshopPartDefinition part = WorkshopCatalog.FindById(entry.Value);
            if (part != null && part.Slot == slot)
                _loadout[slot] = part;
        }

        EnsureRequiredParts();
        _status.Text = loaded ? message : $"{message}  Prototype catalogue loaded.";
    }

    private void EnsureRequiredParts()
    {
        if (!_loadout.ContainsKey(MechPartSlot.Frame))
            _loadout[MechPartSlot.Frame] = WorkshopCatalog.FindById("frame_starter");

        if (!_loadout.ContainsKey(MechPartSlot.Mobility))
            _loadout[MechPartSlot.Mobility] = WorkshopCatalog.FindById("mobility_walker");
    }

    private void OnFilterChanged(long index)
    {
        RefreshCatalogue();
    }

    private void RefreshCatalogue()
    {
        foreach (Node child in _partList.GetChildren())
        {
            _partList.RemoveChild(child);
            child.QueueFree();
        }

        IEnumerable<WorkshopPartDefinition> parts = WorkshopCatalog.All;
        switch (_filter.Selected)
        {
            case 1: parts = parts.Where(part => part.Slot == MechPartSlot.Frame); break;
            case 2: parts = parts.Where(part => part.Slot == MechPartSlot.Mobility); break;
            case 3: parts = parts.Where(part => part.Slot == MechPartSlot.Core); break;
            case 4: parts = parts.Where(part => part.Slot == MechPartSlot.Utility); break;
        }

        foreach (WorkshopPartDefinition part in parts)
        {
            var button = MakeButton($"{part.DisplayName}\n{part.Slot.ToString().ToUpperInvariant()}  •  {part.Weight} WT  •  {part.BuildCost} BC", false);
            button.CustomMinimumSize = new Vector2(0, 54);
            button.Alignment = HorizontalAlignment.Left;
            button.Pressed += () => SelectPart(part);
            _partList.AddChild(button);
        }
    }

    private void SelectPart(WorkshopPartDefinition part)
    {
        _selectedPart = part;
        _selectedName.Text = part.DisplayName;
        _selectedDescription.Text = $"{part.Description}\n\nSlot: {part.Slot}";
        _selectedStats.Text =
            $"HEALTH          {part.Health}\n" +
            $"WEIGHT          {part.Weight}\n" +
            $"BUILD COST      {part.BuildCost}\n" +
            $"ENERGY RESERVE  {part.EnergyCapacity}\n" +
            $"COOLING         {part.Cooling}\n" +
            $"ARMOR           {part.Armor}";
        _equipButton.Disabled = false;
        RefreshBuildSummary();
    }

    private void EquipSelectedPart()
    {
        if (_selectedPart == null || !_selectedPart.HasValidData())
            return;

        var proposed = new Dictionary<MechPartSlot, WorkshopPartDefinition>(_loadout)
        {
            [_selectedPart.Slot] = _selectedPart
        };

        if (!MechBuildRules.TryValidateWorkshopLoadout(
            proposed, MaxWeight, MaxBuildCost, out _, out string validationError))
        {
            _status.Text = $"Cannot equip: {validationError}";
            return;
        }

        _loadout.Clear();
        foreach (KeyValuePair<MechPartSlot, WorkshopPartDefinition> entry in proposed)
            _loadout[entry.Key] = entry.Value;

        PersistAndRefresh("Component equipped.");
    }

    private void RemoveOptionalPart()
    {
        if (_selectedPart == null ||
            _selectedPart.Slot == MechPartSlot.Frame ||
            _selectedPart.Slot == MechPartSlot.Mobility)
        {
            _status.Text = "Frame and mobility components are required.";
            return;
        }

        if (_loadout.Remove(_selectedPart.Slot))
            PersistAndRefresh("Optional component removed.");
        else
            _status.Text = "That slot is already empty.";
    }

    private void ResetBuild()
    {
        _loadout.Clear();
        EnsureRequiredParts();
        PersistAndRefresh("Starter build restored.");
    }

    private void SaveBuild()
    {
        PersistAndRefresh("Build saved.");
    }

    private void PersistAndRefresh(string successMessage)
    {
        var data = new WorkshopLoadoutData
        {
            Version = WorkshopLoadoutCodec.CurrentVersion,
            BuildName = "My Mech",
            EquippedPartIds = _loadout.ToDictionary(entry => entry.Key.ToString(), entry => entry.Value.Id)
        };

        if (WorkshopSaveStore.TrySave(data, out string message))
            _status.Text = successMessage + " " + message;
        else
            _status.Text = message;

        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshCatalogue();
        RefreshBuildSummary();
        RefreshSelectedPart();
    }

    private void RefreshSelectedPart()
    {
        if (_selectedPart == null)
        {
            _selectedName.Text = "Choose a component";
            _selectedDescription.Text = "Select an item from the catalogue to inspect its details.";
            _selectedStats.Text = "";
            _equipButton.Disabled = true;
        }
    }

    private void RefreshBuildSummary()
    {
        bool buildReady = MechBuildRules.TryValidateWorkshopLoadout(
            _loadout, MaxWeight, MaxBuildCost, out WorkshopBuildStats stats, out string validationError);

        _healthValue.Text = stats.Health.ToString();
        _weightValue.Text = $"{stats.Weight} / {MaxWeight}";
        _costValue.Text = $"{stats.BuildCost} / {MaxBuildCost}";
        _energyValue.Text = stats.EnergyCapacity.ToString();
        _coolingValue.Text = stats.Cooling.ToString();
        _armorValue.Text = stats.Armor.ToString();

        _buildStatus.Text = buildReady ? "BUILD READY" : validationError.ToUpperInvariant();
        _buildStatus.AddThemeColorOverride("font_color",
            buildReady ? new Color(0.50f, 0.82f, 0.66f) : new Color(0.92f, 0.65f, 0.43f));

        _preview.SetLoadout(_loadout);
        _loadoutSummary.Text =
            $"FRAME  {GetEquippedName(MechPartSlot.Frame)}     MOBILITY  {GetEquippedName(MechPartSlot.Mobility)}\n" +
            $"CORE  {GetEquippedName(MechPartSlot.Core)}     UTILITY  {GetEquippedName(MechPartSlot.Utility)}";
        _removeButton.Disabled = _selectedPart == null ||
                                 _selectedPart.Slot == MechPartSlot.Frame ||
                                 _selectedPart.Slot == MechPartSlot.Mobility ||
                                 !_loadout.ContainsKey(_selectedPart.Slot);
    }

    private string GetEquippedName(MechPartSlot slot)
    {
        return _loadout.TryGetValue(slot, out WorkshopPartDefinition part)
            ? part.DisplayName
            : "EMPTY";
    }

    private static Label AddStatLine(VBoxContainer parent, string name)
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        var label = NewLabel(name, 10, new Color(0.54f, 0.66f, 0.70f));
        label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var value = NewLabel("0", 13, Colors.White, true);
        value.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(label);
        row.AddChild(value);
        parent.AddChild(row);
        return value;
    }

    private static void AddDivider(VBoxContainer parent)
    {
        var line = new HSeparator();
        line.AddThemeColorOverride("separator", new Color(0.18f, 0.26f, 0.30f));
        parent.AddChild(line);
    }

    private static PanelContainer MakePanel()
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.065f, 0.09f, 0.11f),
            BorderColor = new Color(0.16f, 0.23f, 0.27f),
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 12,
            ContentMarginBottom = 12
        };
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

    private static VBoxContainer PanelColumn(PanelContainer panel)
    {
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", 10);
        panel.AddChild(column);
        return column;
    }

    private static Label NewLabel(string text, int size, Color color, bool bold = false)
    {
        var label = new Label { Text = text };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        if (bold)
            label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.3f));
        return label;
    }

    private static Button MakeButton(string text, bool primary)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 38),
            FocusMode = FocusModeEnum.All
        };

        Color baseColor = primary ? new Color(0.20f, 0.43f, 0.48f) : new Color(0.12f, 0.18f, 0.21f);
        var normal = new StyleBoxFlat
        {
            BgColor = baseColor,
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomLeft = 5,
            CornerRadiusBottomRight = 5,
            ContentMarginLeft = 10,
            ContentMarginRight = 10,
            ContentMarginTop = 6,
            ContentMarginBottom = 6
        };
        var hover = (StyleBoxFlat)normal.Duplicate();
        hover.BgColor = baseColor.Lightened(0.12f);
        var pressed = (StyleBoxFlat)normal.Duplicate();
        pressed.BgColor = baseColor.Darkened(0.12f);

        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeColorOverride("font_color", Colors.White);
        button.AddThemeColorOverride("font_hover_color", Colors.White);
        return button;
    }
}
