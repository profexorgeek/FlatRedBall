﻿using Gum.Wireframe;
using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall.Gui;

namespace FlatRedBall.Forms.Controls
{
    public class ScrollViewer : FrameworkElement
    {
        #region Fields/Properties

        bool reactToInnerPanelPositionOrSizeChanged = true;

        protected ScrollBar verticalScrollBar;

        GraphicalUiElement innerPanel;
        public GraphicalUiElement InnerPanel => innerPanel;

        protected GraphicalUiElement clipContainer;

        #endregion

        #region Initialize

        public ScrollViewer() : base() { }

        public ScrollViewer(GraphicalUiElement visual) : base(visual) { }

        protected override void ReactToVisualChanged()
        {
            var scrollBarVisual = Visual.GetGraphicalUiElementByName("VerticalScrollBarInstance"); 
            if(scrollBarVisual.FormsControlAsObject == null)
            {
                verticalScrollBar = new ScrollBar(scrollBarVisual);
            }
            else
            {
                verticalScrollBar = scrollBarVisual.FormsControlAsObject as ScrollBar;
            }
            verticalScrollBar.ValueChanged += HandleVerticalScrollBarValueChanged;
            // Depending on the height and width units, the scroll bar may get its update
            // called before or after this. We can't bet on the order, so we have to handle
            // both this and the scroll bar's height value changes, and adjust according to both:
            var thumbVisual =
                verticalScrollBar.Visual.GetGraphicalUiElementByName("ThumbInstance");
            verticalScrollBar.Visual.SizeChanged += HandleVerticalScrollBarThumbSizeChanged;


            innerPanel = Visual.GetGraphicalUiElementByName("InnerPanelInstance");
            innerPanel.SizeChanged += HandleInnerPanelSizeChanged;
            innerPanel.PositionChanged += HandleInnerPanelPositionChanged;
            clipContainer = Visual.GetGraphicalUiElementByName("ClipContainerInstance");

            Visual.MouseWheelScroll += HandleMouseWheelScroll;
            Visual.RollOverBubbling += HandleRollOver;
            Visual.SizeChanged += HandleVisualSizeChanged;

            UpdateVerticalScrollBarValues();

            base.ReactToVisualChanged();
        }

        private void HandleRollOver(IWindow window, RoutedEventArgs args)
        {
            if(GuiManager.Cursor.PrimaryDown && GuiManager.Cursor.LastInputDevice == InputDevice.TouchScreen)
            {
                verticalScrollBar.Value -= GuiManager.Cursor.ScreenYChange /
                    global::RenderingLibrary.SystemManagers.Default.Renderer.Camera.Zoom;
                args.Handled = true;
            }
        }

        private void HandleMouseWheelScroll(IWindow window, FlatRedBall.Gui.RoutedEventArgs args)
        {
            var valueBefore = verticalScrollBar.Value;

            const float scrollMultiplier = 12;

            // Do we want to use the small change? Or have some separate value that the user can set?
            verticalScrollBar.Value -= GuiManager.Cursor.ZVelocity * verticalScrollBar.SmallChange;

            args.Handled = verticalScrollBar.Value != valueBefore;
        }


        #endregion

        #region Event Handlers

        private void HandleVerticalScrollBarValueChanged(object sender, EventArgs e)
        {
            reactToInnerPanelPositionOrSizeChanged = false;
            innerPanel.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
            innerPanel.Y = -(float)verticalScrollBar.Value;
            reactToInnerPanelPositionOrSizeChanged = true;
        }

        private void HandleInnerPanelSizeChanged(object sender, EventArgs e)
        {
            if(reactToInnerPanelPositionOrSizeChanged)
            {
                UpdateVerticalScrollBarValues();
            }
        }

        private void HandleVisualSizeChanged(object sender, EventArgs args)
        {
            if(reactToInnerPanelPositionOrSizeChanged)
            {
                UpdateVerticalScrollBarValues();
            }
        }

        private void HandleVerticalScrollBarThumbSizeChanged(object sender, EventArgs args)
        {
            if (reactToInnerPanelPositionOrSizeChanged)
            {
                UpdateVerticalScrollBarValues();
            }
        }

        private void HandleInnerPanelPositionChanged(object sender, EventArgs e)
        {
            if(reactToInnerPanelPositionOrSizeChanged)
            {
                UpdateVerticalScrollBarValues();
            }
        }
        #endregion

        #region UpdateTo methods

        // Currently this is public because Gum objects don't have events
        // when positions and sizes change. Eventually, we'll have this all
        // handled internally and this can be made private.
        public void UpdateVerticalScrollBarValues()
        {
            verticalScrollBar.Minimum = 0;
            verticalScrollBar.ViewportSize = clipContainer.GetAbsoluteHeight();
            var maxValue = innerPanel.GetAbsoluteHeight() - clipContainer.GetAbsoluteHeight();

            maxValue = System.Math.Max(0, maxValue);

            verticalScrollBar.Maximum = maxValue;

            verticalScrollBar.SmallChange = 10;
            verticalScrollBar.LargeChange = verticalScrollBar.ViewportSize;
        }

        #endregion
    }
}
