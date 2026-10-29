using System.Collections.ObjectModel;
using yuyiAvalonia.UI.ElementControls.Themes;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls
{
    public class UIComponentDefinitions
    {
        /// <summary>
        /// 获取主题提供器
        /// </summary>
        private static ThemeProvider ThemeProvider => ThemeProvider.Instance;

        /// <summary>
        /// 从主题获取指定组件类型的基础样式
        /// </summary>
        private static ComponentThemeStyle GetBaseStyle(ComponentType type)
        {
            return ThemeProvider.GetStyle(type, ComponentState.Normal);
        }

        /// <summary>
        /// 从主题样式创建组件默认值
        /// </summary>
        private static ComponentDefaults CreateDefaultsFromTheme(ComponentType type, Action<ComponentDefaults>? customizer = null)
        {
            var themeStyle = GetBaseStyle(type);
            var defaults = ComponentDefaults.FromThemeStyle(themeStyle);
            customizer?.Invoke(defaults);
            return defaults;
        }

        public static ObservableCollection<ComponentDefinition> LoadWeChatComponentDefinitions()
        {
            ObservableCollection<ComponentDefinition> ComponentDefinitions = new ObservableCollection<ComponentDefinition>();

            #region ===== Grid 布局 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "均分2列",
                Description = "两列等宽布局",
                Type = ComponentType.Grid,
                Category = "布局",
                Icon = "grid_2col",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Grid, d =>
                {
                    d.Width = 350;
                    d.Height = 100;
                    d.CornerRadius = 0;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "均分3列",
                Description = "三列等宽布局",
                Type = ComponentType.Grid,
                Category = "布局",
                Icon = "grid_3col",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Grid, d =>
                {
                    d.Width = 350;
                    d.Height = 100;
                    d.CornerRadius = 0;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "均分4列",
                Description = "四列等宽布局",
                Type = ComponentType.Grid,
                Category = "布局",
                Icon = "grid_4col",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Grid, d =>
                {
                    d.Width = 350;
                    d.Height = 80;
                    d.CornerRadius = 0;
                })
            });

            #endregion

            #region ===== 容器组件 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "垂直容器",
                Description = "垂直排列子元素",
                Type = ComponentType.VerticalStackPanel,
                Category = "容器",
                Icon = "container_v",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.VerticalStackPanel, d =>
                {
                    d.Width = 350;
                    d.Height = 200;
                    d.CornerRadius = 0;
                    d.Padding = 5;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "水平容器",
                Description = "水平排列子元素",
                Type = ComponentType.HorizontalStackPanel,
                Category = "容器",
                Icon = "container_h",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.HorizontalStackPanel, d =>
                {
                    d.Width = 350;
                    d.Height = 50;
                    d.CornerRadius = 0;
                    d.Padding = 5;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "Panel容器",
                Description = "基础面板容器",
                Type = ComponentType.Panel,
                Category = "容器",
                Icon = "container_panel",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Panel, d =>
                {
                    d.Width = 350;
                    d.Height = 100;
                    d.CornerRadius = 0;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "Grid容器",
                Description = "网格布局容器",
                Type = ComponentType.Grid,
                Category = "容器",
                Icon = "container_grid",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Grid, d =>
                {
                    d.Width = 350;
                    d.Height = 150;
                    d.CornerRadius = 0;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "Canvas容器",
                Description = "绝对定位画布",
                Type = ComponentType.Canvas,
                Category = "容器",
                Icon = "container_canvas",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Canvas, d =>
                {
                    d.Width = 350;
                    d.Height = 200;
                    d.CornerRadius = 0;
                })
            });

            #endregion

            #region ===== 文本组件 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "页面标题",
                Description = "页面大标题",
                Type = ComponentType.Text,
                Category = "文本",
                Icon = "text_title",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Text, d =>
                {
                    d.Width = 300;
                    d.Height = 40;
                    d.FontSize = 20;
                    d.IsBold = true;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "普通文本",
                Description = "普通正文文本",
                Type = ComponentType.Text,
                Category = "文本",
                Icon = "text_normal",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Text, d =>
                {
                    d.Width = 335;
                    d.Height = 30;
                    d.FontSize = 14;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "副文本",
                Description = "次要信息文本",
                Type = ComponentType.Text,
                Category = "文本",
                Icon = "text_sub",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Text, d =>
                {
                    d.Width = 200;
                    d.Height = 24;
                    d.FontSize = 12;
                })
            });

            #endregion

            #region ===== 卡片组件 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "通用卡片",
                Description = "白色背景卡片",
                Type = ComponentType.Card,
                Category = "卡片",
                Icon = "card_normal",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Card, d =>
                {
                    d.Width = 355;
                    d.Height = 100;
                    d.CornerRadius = 8;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角卡片",
                Description = "大圆角白色卡片",
                Type = ComponentType.Card,
                Category = "卡片",
                Icon = "card_round",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Card, d =>
                {
                    d.Width = 355;
                    d.Height = 120;
                    d.CornerRadius = 12;
                })
            });

            #endregion

            #region ===== 按钮组件 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "主按钮",
                Description = "微信绿色主按钮",
                Type = ComponentType.Button,
                Category = "按钮",
                Icon = "btn_primary",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Button, d =>
                {
                    d.Width = 335;
                    d.Height = 44;
                    d.BackgroundColor = "#07C160";
                    d.ForeColor = "#FFFFFF";
                    d.FontSize = 16;
                    d.CornerRadius = 0;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "次要按钮",
                Description = "边框次要按钮",
                Type = ComponentType.Button,
                Category = "按钮",
                Icon = "btn_secondary",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Button, d =>
                {
                    d.Width = 335;
                    d.Height = 44;
                    d.BackgroundColor = "#FFFFFF";
                    d.ForeColor = "#07C160";
                    d.BorderColor = "#07C160";
                    d.FontSize = 16;
                    d.CornerRadius = 0;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "文字按钮",
                Description = "文字链接按钮",
                Type = ComponentType.Button,
                Category = "按钮",
                Icon = "btn_text",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Button, d =>
                {
                    d.Width = 80;
                    d.Height = 30;
                    d.BackgroundColor = "Transparent";
                    d.ForeColor = "#07C160";
                    d.FontSize = 14;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角按钮",
                Description = "全圆角主按钮",
                Type = ComponentType.Button,
                Category = "按钮",
                Icon = "btn_round",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Button, d =>
                {
                    d.Width = 200;
                    d.Height = 40;
                    d.BackgroundColor = "#07C160";
                    d.ForeColor = "#FFFFFF";
                    d.FontSize = 16;
                    d.CornerRadius = 20;
                })
            });

            #endregion

            #region ===== 媒体组件 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "头像",
                Description = "用户头像",
                Type = ComponentType.Image,
                Category = "媒体",
                Icon = "media_avatar",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Image, d =>
                {
                    d.Width = 60;
                    d.Height = 60;
                    d.ImageStretch = Avalonia.Media.Stretch.UniformToFill;
                    d.CornerRadius = 30;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "图标按钮",
                Description = "圆形图标按钮",
                Type = ComponentType.Button,
                Category = "按钮",
                Icon = "media_icon",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Button, d =>
                {
                    d.Width = 50;
                    d.Height = 50;
                    d.BackgroundColor = "#F7F7F7";
                    d.ForeColor = "#666666";
                    d.CornerRadius = 25;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "徽标",
                Description = "数字徽章",
                Type = ComponentType.Card,
                Category = "媒体",
                Icon = "badge_number",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Card, d =>
                {
                    d.Width = 16;
                    d.Height = 16;
                    d.BackgroundColor = "#FF3B30";
                    d.CornerRadius = 8;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "图片",
                Description = "图片显示控件",
                Type = ComponentType.Image,
                Category = "媒体",
                Icon = "media_image",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Image, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                    d.ImageStretch = Avalonia.Media.Stretch.Uniform;
                })
            });

            #endregion

            #region ===== 流程图形状 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "三角形",
                Description = "可调节点三角形",
                Type = ComponentType.Triangle,
                Category = "流程图形状",
                Icon = "shape_triangle",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Triangle, d =>
                {
                    d.Width = 100;
                    d.Height = 100;
                    d.BackgroundColor = "#FF6B6B";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "矩形",
                Description = "基础矩形图形",
                Type = ComponentType.Rectangle,
                Category = "流程图形状",
                Icon = "shape_rect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Rectangle, d =>
                {
                    d.Width = 100;
                    d.Height = 60;
                    d.BackgroundColor = "#4ECDC4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角矩形",
                Description = "可调每个角圆角的矩形",
                Type = ComponentType.RoundedRectangle,
                Category = "流程图形状",
                Icon = "shape_roundedrect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.RoundedRectangle, d =>
                {
                    d.Width = 100;
                    d.Height = 60;
                    d.BackgroundColor = "#4ECDC4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆形",
                Description = "椭圆/圆形图形",
                Type = ComponentType.Ellipse,
                Category = "流程图形状",
                Icon = "shape_circle",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Ellipse, d =>
                {
                    d.Width = 100;
                    d.Height = 100;
                    d.BackgroundColor = "#45B7D1";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "平行四边形",
                Description = "倾斜平行四边形",
                Type = ComponentType.Parallelogram,
                Category = "流程图形状",
                Icon = "shape_parallelogram",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Parallelogram, d =>
                {
                    d.Width = 120;
                    d.Height = 80;
                    d.BackgroundColor = "#96CEB4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "六星形",
                Description = "六角星形（可调角数与各角圆角）",
                Type = ComponentType.Star,
                Category = "流程图形状",
                Icon = "shape_sixstar",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Star, d =>
                {
                    d.Width = 120;
                    d.Height = 120;
                    d.BackgroundColor = "#9C27B0";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角多边形",
                Description = "圆角三角形/多边形（可调边数与各角圆角）",
                Type = ComponentType.RoundedPolygon,
                Category = "流程图形状",
                Icon = "shape_roundedpolygon",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.RoundedPolygon, d =>
                {
                    d.Width = 120;
                    d.Height = 120;
                    d.BackgroundColor = "#FF9F43";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "扇形 / 椭圆",
                Description = "可调起止角、内圈占比，4 种形态（椭圆/扇形/圆环/扇环）",
                Type = ComponentType.Sector,
                Category = "流程图形状",
                Icon = "shape_sector",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Sector, d =>
                {
                    d.Width = 140;
                    d.Height = 140;
                    d.BackgroundColor = "#7B61FF";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            #endregion

            #region ===== 图形容器 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "三角形",
                Description = "可在三角形内放置内容",
                Type = ComponentType.Triangle,
                Category = "流程图形状",
                Icon = "shape_triangle",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Triangle, d =>
                {
                    d.Width = 120;
                    d.Height = 100;
                    d.BackgroundColor = "#FF6B6B";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "矩形",
                Description = "可在矩形内放置内容",
                Type = ComponentType.Rectangle,
                Category = "流程图形状",
                Icon = "shape_rect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Rectangle, d =>
                {
                    d.Width = 120;
                    d.Height = 80;
                    d.BackgroundColor = "#4ECDC4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角矩形",
                Description = "可调 4 个角圆角的矩形容器",
                Type = ComponentType.RoundedRectangle,
                Category = "流程图形状",
                Icon = "shape_roundedrect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.RoundedRectangle, d =>
                {
                    d.Width = 120;
                    d.Height = 80;
                    d.BackgroundColor = "#4ECDC4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆形",
                Description = "可在圆形内放置内容",
                Type = ComponentType.Ellipse,
                Category = "流程图形状",
                Icon = "shape_circle",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Ellipse, d =>
                {
                    d.Width = 120;
                    d.Height = 100;
                    d.BackgroundColor = "#45B7D1";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "平行四边形",
                Description = "可在平行四边形内放置内容",
                Type = ComponentType.Parallelogram,
                Category = "流程图形状",
                Icon = "shape_parallelogram",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Parallelogram, d =>
                {
                    d.Width = 140;
                    d.Height = 80;
                    d.BackgroundColor = "#96CEB4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            #endregion

            return ComponentDefinitions;
        }

        public static ObservableCollection<ComponentDefinition> LoadWebComponentDefinitions()
        {
            ObservableCollection<ComponentDefinition> ComponentDefinitions = new ObservableCollection<ComponentDefinition>();

            #region ===== 布局组件 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "垂直容器",
                Description = "垂直排列子元素",
                Type = ComponentType.VerticalStackPanel,
                Category = "布局组件",
                Icon = "container_v",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.VerticalStackPanel, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                    d.Padding = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "水平容器",
                Description = "水平排列子元素",
                Type = ComponentType.HorizontalStackPanel,
                Category = "布局组件",
                Icon = "container_h",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.HorizontalStackPanel, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                    d.Padding = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "网格容器",
                Description = "网格布局容器",
                Type = ComponentType.Grid,
                Category = "布局组件",
                Icon = "container_grid",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Grid, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "画布容器",
                Description = "绝对定位画布",
                Type = ComponentType.Canvas,
                Category = "布局组件",
                Icon = "container_canvas",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Canvas, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "层叠容器",
                Description = "基础面板容器",
                Type = ComponentType.Panel,
                Category = "布局组件",
                Icon = "container_panel",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Panel, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角矩形容器",
                Description = "可调 4 角圆角的容器（内置 StackPanel，按 Orientation/Spacing 排列子元素）",
                Type = ComponentType.RoundedRectangleStackPanel,
                Category = "布局组件",
                Icon = "shape_roundedrect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.RoundedRectangleStackPanel, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                    d.CornerRadius = 8;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角画布容器",
                Description = "可调 4 角圆角的容器（内置 Canvas，子元素按 Left/Top 自由定位）",
                Type = ComponentType.RoundedRectangleCanvas,
                Category = "布局组件",
                Icon = "shape_roundedrect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.RoundedRectangleCanvas, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                    d.CornerRadius = 8;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角网格容器",
                Description = "可调 4 角圆角的容器（内置 Grid，子元素按 Row/Column 落入单元格）",
                Type = ComponentType.RoundedRectangleGrid,
                Category = "布局组件",
                Icon = "shape_roundedrect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.RoundedRectangleGrid, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                    d.CornerRadius = 8;
                })
            });

            #endregion

            #region ===== 基础组件 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "文本",
                Description = "显示文本内容",
                Type = ComponentType.Text,
                Category = "基础组件",
                Icon = "text_normal",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Text, d =>
                {
                    d.Width = 180;
                    d.Height = 40;
                    d.FontSize = 14;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "按钮",
                Description = "可点击的按钮",
                Type = ComponentType.Button,
                Category = "基础组件",
                Icon = "btn_primary",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Button, d =>
                {
                    d.Width = 120;
                    d.Height = 40;
                    d.Padding = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "图片",
                Description = "显示图片",
                Type = ComponentType.Image,
                Category = "基础组件",
                Icon = "media_image",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Image, d =>
                {
                    d.Width = 120;
                    d.Height = 120;
                    d.ImageStretch = Avalonia.Media.Stretch.Uniform;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "卡片",
                Description = "带边框和阴影的容器",
                Type = ComponentType.Card,
                Category = "基础组件",
                Icon = "card_normal",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Card, d =>
                {
                    d.Width = 300;
                    d.Height = 200;
                    d.Padding = 16;
                })
            });

            #endregion

            #region ===== 流程图形状 =====

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "三角形",
                Description = "可在三角形内放置内容",
                Type = ComponentType.Triangle,
                Category = "流程图形状",
                Icon = "shape_triangle",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Triangle, d =>
                {
                    d.Width = 120;
                    d.Height = 100;
                    d.BackgroundColor = "#FF6B6B";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "矩形",
                Description = "可在矩形内放置内容",
                Type = ComponentType.Rectangle,
                Category = "流程图形状",
                Icon = "shape_rect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Rectangle, d =>
                {
                    d.Width = 120;
                    d.Height = 80;
                    d.BackgroundColor = "#4ECDC4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角矩形",
                Description = "可调 4 个角圆角的矩形容器",
                Type = ComponentType.RoundedRectangle,
                Category = "流程图形状",
                Icon = "shape_roundedrect",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.RoundedRectangle, d =>
                {
                    d.Width = 120;
                    d.Height = 80;
                    d.BackgroundColor = "#4ECDC4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆形",
                Description = "可在圆形内放置内容",
                Type = ComponentType.Ellipse,
                Category = "流程图形状",
                Icon = "shape_circle",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Ellipse, d =>
                {
                    d.Width = 120;
                    d.Height = 100;
                    d.BackgroundColor = "#45B7D1";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "平行四边形",
                Description = "可在平行四边形内放置内容",
                Type = ComponentType.Parallelogram,
                Category = "流程图形状",
                Icon = "shape_parallelogram",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Parallelogram, d =>
                {
                    d.Width = 140;
                    d.Height = 80;
                    d.BackgroundColor = "#96CEB4";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 2;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "星形",
                Description = "可在星形内放置内容",
                Type = ComponentType.Star,
                Category = "流程图形状",
                Icon = "shape_sixstar",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Star, d =>
                {
                    d.Width = 140;
                    d.Height = 140;
                    d.BackgroundColor = "#9C27B0";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 1;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "圆角多边形",
                Description = "可在圆角多边形内放置内容（可调边数与各角圆角）",
                Type = ComponentType.RoundedPolygon,
                Category = "流程图形状",
                Icon = "shape_roundedpolygon",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.RoundedPolygon, d =>
                {
                    d.Width = 120;
                    d.Height = 120;
                    d.BackgroundColor = "#FF9F43";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 1;
                })
            });

            ComponentDefinitions.Add(new ComponentDefinition
            {
                Name = "扇形 / 椭圆",
                Description = "可在扇形 / 椭圆内放置内容（可调起止角、内圈占比）",
                Type = ComponentType.Sector,
                Category = "流程图形状",
                Icon = "shape_sector",
                ThemeId = "default",
                Defaults = CreateDefaultsFromTheme(ComponentType.Sector, d =>
                {
                    d.Width = 140;
                    d.Height = 140;
                    d.BackgroundColor = "#7B61FF";
                    d.StrokeColor = "#333333";
                    d.StrokeThickness = 1;
                })
            });

            #endregion

            return ComponentDefinitions;
        }
    }
}
