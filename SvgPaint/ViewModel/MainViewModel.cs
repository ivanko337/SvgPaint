﻿using SvgPaint.Controls;
using SvgPaint.Model;
using Svg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Resources;
using System.Xml.Linq;
using SvgPaint.Extensions;

namespace SvgPaint.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

        #region Member

        public InkCanvas inkCanvas { get; set; }
        public DrawStrokeBase curDraw { get; set; }  //current draw stroke

        private StrokeCollection lstStrokeClipBoard; //clipboard of strokes

        private DoCommandStack doCmdStack;           //redo undo command stack

        private int editingOperationCount;           //redo undo command count

        private bool isDraw;                         // is drawing stroke

        private ModeEnum curOperationMode;
        private ColorModeEnum curColorMode;
        private bool curColorPickerMode;

        private Brush foreground;

        public Brush Foreground
        {
            get { return foreground; }
            set
            {
                var old_foreground = foreground;

                foreground = value;
                var color = ((SolidColorBrush)foreground).Color;
                inkCanvas.DefaultDrawingAttributes.Color = color;
                var lstStrokes = inkCanvas.GetSelectedStrokes();

                if (lstStrokes.Count > 0)
                {
                    foreach (var stroke in lstStrokes)
                    {
                        stroke.DrawingAttributes.Color = color;
                    }

                    editingOperationCount++;
                    CommandItem item = new SelectionColorOrWidthCI(doCmdStack, lstStrokes, old_foreground, foreground,
                        Background, Background, StrokeWidth, StrokeWidth, editingOperationCount);
                    doCmdStack.Enqueue(item);
                }
                OnPropertyChanged("Foreground");
            }
        }

        private Brush background;

        public Brush Background
        {
            get { return background; }
            set
            {
                var old_background = background;

                background = value;

                inkCanvas.DefaultDrawingAttributes.AddPropertyData(DrawAttributesGuid.BackgroundColor, background.ToString());

                var lstStrokes = inkCanvas.GetSelectedStrokes();

                if (lstStrokes.Count > 0)
                {
                    foreach (var stroke in lstStrokes)
                    {
                        //get DrawingAttributes and set DrawingAttributes to trigger AttributesChanged event
                        var attr = stroke.DrawingAttributes;
                        attr.AddPropertyData(DrawAttributesGuid.BackgroundColor, background.ToString());

                        stroke.DrawingAttributes = attr;
                    }
                    editingOperationCount++;
                    CommandItem item = new SelectionColorOrWidthCI(doCmdStack, lstStrokes, Foreground, Foreground,
                       old_background, background, StrokeWidth, StrokeWidth, editingOperationCount);
                    doCmdStack.Enqueue(item);
                }
                OnPropertyChanged("Background");
            }
        }

        private Color selectColor;

        public Color SelectColor
        {
            get { return selectColor; }
            set
            {
                selectColor = value;
                if (curColorMode == ColorModeEnum.Background)
                {
                    Background = new SolidColorBrush(selectColor);
                }
                else if (curColorMode == ColorModeEnum.Foreground)
                {
                    Foreground = new SolidColorBrush(selectColor);
                }
                OnPropertyChanged("SelectColor");
            }
        }



        private int penWidthIndex;

        public int PenWidthIndex
        {
            get { return penWidthIndex; }
            set
            {
                var old_strokewidth = StrokeWidth;

                penWidthIndex = value;

                inkCanvas.DefaultDrawingAttributes.Height = inkCanvas.DefaultDrawingAttributes.Width = StrokeWidth;

                var lstStrokes = inkCanvas.GetSelectedStrokes();

                if (lstStrokes.Count > 0)
                {
                    foreach (var stroke in lstStrokes)
                    {
                        stroke.DrawingAttributes.Height = stroke.DrawingAttributes.Width = StrokeWidth;
                    }
                    editingOperationCount++;
                    CommandItem item = new SelectionColorOrWidthCI(doCmdStack, lstStrokes, Foreground, Foreground,
                      Background, Background, old_strokewidth, StrokeWidth, editingOperationCount);
                    doCmdStack.Enqueue(item);
                }

                OnPropertyChanged("PenWidthIndex");
            }
        }


        private int StrokeWidth
        {
            get { return (penWidthIndex + 1) * 2; }
        }


        private string mouseLocationText;            //show mouse location

        public string MouseLocationText
        {
            get { return mouseLocationText; }
            set
            {
                mouseLocationText = value;
                OnPropertyChanged("MouseLocationText");
            }
        }

        private string fieldSizeText;                //show stroke field size

        public string FieldSizeText
        {
            get { return fieldSizeText; }
            set
            {

                fieldSizeText = value;
                OnPropertyChanged("FieldSizeText");
            }
        }

        private string canvasSizeText;                //show canvas size

        public string CanvasSizeText
        {
            get { return canvasSizeText; }
            set
            {
                canvasSizeText = value;
                OnPropertyChanged("CanvasSizeText");
            }
        }

        private string fileSizeText;                  //show file size

        public string FileSizeText
        {
            get { return fileSizeText; }
            set
            {
                fileSizeText = value;
                OnPropertyChanged("FileSizeText");
            }
        }

        private Visibility isSavedVisible = Visibility.Collapsed;   //is saved visibillity

        public Visibility IsSavedVisible
        {
            get { return isSavedVisible; }
            set
            {
                isSavedVisible = value;
                OnPropertyChanged("IsSavedVisible");
            }
        }

        private bool isOpenEditColor;

        public bool IsOpenEditColor
        {
            get { return isOpenEditColor; }
            set
            {
                isOpenEditColor = value;
                OnPropertyChanged("IsOpenEditColor");
            }
        }

        #endregion

        #region Function

        public MainViewModel(InkCanvas _inkCanvas)
        {
            inkCanvas = _inkCanvas;
            inkCanvas.PreviewMouseLeftButtonDown += CanvasMouseDown;
            inkCanvas.MouseMove += CanvasMouseMove;
            inkCanvas.MouseLeave += CanvasMouseLeave;
            inkCanvas.MouseUp += CanvasMouseUp;
            inkCanvas.SelectionMoving += Canvas_SelectionMovingOrResizing;
            inkCanvas.SelectionResizing += Canvas_SelectionMovingOrResizing;
            inkCanvas.SizeChanged += InkCanvas_SizeChanged;

            doCmdStack = new DoCommandStack(_inkCanvas.Strokes);
            lstStrokeClipBoard = new StrokeCollection();

            //Init
            PenWidthIndex = 0;
            Foreground = Brushes.Black;
            Background = Brushes.White;
            curColorMode = ColorModeEnum.Foreground;
            curColorPickerMode = false;
            SelectColor = Colors.Black;

            PenMode();
        }


        #region Command
        public void Delete()
        {
            var lstStokes = inkCanvas.GetSelectedStrokes();
            if (lstStokes.Count > 0)
            {
                Strokes_Removed(lstStokes);
                inkCanvas.Strokes.Remove(lstStokes);
            }
        }
        
        public void Cut()
        {
            var lstStokes = inkCanvas.GetSelectedStrokes();
            if (lstStokes.Count > 0)
            {
                lstStrokeClipBoard.Clear();
                lstStrokeClipBoard.Add(lstStokes);
                Strokes_Removed(lstStokes);
                inkCanvas.Strokes.Remove(lstStokes);
            }
        }

        public void Copy()
        {
            var lstStokes = inkCanvas.GetSelectedStrokes();
            if (lstStokes.Count > 0)
            {
                lstStrokeClipBoard.Clear();
                lstStrokeClipBoard.Add(lstStokes);
            }
        }

        public void ExportSVG()
        {
            using (var fs = new StreamWriter(@"E:\test.svg"))
            {
                fs.WriteLine("<svg fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\">");

                foreach (var stroke in inkCanvas.Strokes)
                {
                    var points = stroke.StylusPoints;

                    var shape = stroke as StrokeBase;

                    fs.WriteLine(shape?.GetSvgMarkup() ?? stroke.SvgMarkup());
                }

                fs.WriteLine("</svg>");
            }
        }

        public void Paste()
        {
            if (lstStrokeClipBoard.Count > 0)
            {
                var newLstStrokes = lstStrokeClipBoard.Clone();
                Strokes_Added(newLstStrokes);
                inkCanvas.Strokes.Add(newLstStrokes);
                inkCanvas.Select(newLstStrokes);
            }
        }

        public void Undo()
        {
            doCmdStack.Undo();
        }

        /// <summary>
        /// Redo the last edit.
        /// </summary> 
        public void Redo()
        {
            doCmdStack.Redo();
        }

        #endregion

        #region Tool

        /// <summary>
        /// rectangle seletc mode
        /// </summary>
        public void RecSelectMode()
        {
            curDraw = null;
            curOperationMode = ModeEnum.Select;
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
        }

        public void TextMode()
        {
            // curDraw = new DrawPen();
        }

        public void PenMode()
        {
            curDraw = null;
            curOperationMode = ModeEnum.Pen;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        public void EraseByStrokeMode()
        {
            curDraw = null;
            curOperationMode = ModeEnum.Eraser;
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        public void ColorPickerMode()
        {
            //curDraw = null;
            curColorPickerMode = true;
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
        }

        public void DrawLine()
        {
            curOperationMode = ModeEnum.Draw;
            curDraw = new DrawLine();
        }

        public void DrawEllipse()
        {
            curOperationMode = ModeEnum.Draw;
            curDraw = new DrawEllipse();
        }
        public void DrawRectangle()
        {
            curOperationMode = ModeEnum.Draw;
            curDraw = new DrawRectangle();
        }

        public void DrawTriangle()
        {
            curOperationMode = ModeEnum.Draw;
            curDraw = new DrawTriangle();
        }

        public void DrawArrow()
        {
            curOperationMode = ModeEnum.Draw;
            curDraw = new DrawArrowLine();
        }

        public void ForegroundMode()
        {
            curColorMode = ColorModeEnum.Foreground;
        }

        public void BackgroundMode()
        {
            curColorMode = ColorModeEnum.Background;
        }

        public void EditColorMode()
        {
            IsOpenEditColor = true;
        }

        public void SelectColorClick(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button)
            {
                Button btn = e.Source as Button;
                if (curColorMode == ColorModeEnum.Foreground)
                {
                    Foreground = btn.Background;
                }
                else if (curColorMode == ColorModeEnum.Background)
                {
                    Background = btn.Background;
                }
            }
        }


        #endregion

        #region Event
        public void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.C) && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Copy();
            }
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.X) && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Cut();
            }
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.V) && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Paste();
            }
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Y) && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Redo();
            }
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Z) && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Undo();
            }
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Delete))
            {
                Delete();
            }
            e.Handled = true;
        }
        private void Strokes_Added(StrokeCollection lstAdded)
        {
            editingOperationCount++;
            CommandItem item = new StrokesAddedOrRemovedCI(doCmdStack, inkCanvas.EditingMode, lstAdded, new StrokeCollection(), editingOperationCount);
            doCmdStack.Enqueue(item);
        }

        private void Strokes_Removed(StrokeCollection lstRemoved)
        {
            editingOperationCount++;
            CommandItem item = new StrokesAddedOrRemovedCI(doCmdStack, inkCanvas.EditingMode, new StrokeCollection(), lstRemoved, editingOperationCount);
            doCmdStack.Enqueue(item);
        }

        private void InkCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CanvasSizeText = inkCanvas.ActualWidth + "," + inkCanvas.ActualHeight + " пикселей";
        }

        private void Canvas_SelectionMovingOrResizing(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            Rect newRect = e.NewRectangle; Rect oldRect = e.OldRectangle;

            if (newRect.Top < 0d || newRect.Left < 0d)
            {
                Rect newRect2 =
                    new Rect(newRect.Left < 0d ? 0d : newRect.Left,
                                newRect.Top < 0d ? 0d : newRect.Top,
                                newRect.Width,
                                newRect.Height);

                e.NewRectangle = newRect2;
            }

            FieldSizeText = (int)newRect.Width + "," + (int)newRect.Height + " пикселей";
            editingOperationCount++;
            CommandItem item = new SelectionMovedOrResizedCI(doCmdStack, inkCanvas.GetSelectedStrokes(), newRect, oldRect, editingOperationCount);
            doCmdStack.Enqueue(item);
        }

        private void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (inkCanvas.EditingMode == InkCanvasEditingMode.Select && inkCanvas.GetSelectedStrokes().Count > 0)
            {
                Rect rect = inkCanvas.GetSelectionBounds();  //bound of strokes
                rect.Inflate(14, 14);                        //bound of resize thumb
                if (!rect.Contains(e.GetPosition(inkCanvas)))
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                }
                else
                {
                    return;
                }
            }

            if (curColorPickerMode)
            {
                curColorPickerMode = false;

                Mouse.OverrideCursor = Cursors.Arrow;
                POINT p;
                GetCursorPos(out p);
                IntPtr hdc = GetDC(IntPtr.Zero);
                int c = GetPixel(hdc, p.X, p.Y);
                byte r = (byte)(c & 0xFF);
                byte g = (byte)((c & 0xFF00) >> 8);
                byte b = (byte)((c & 0xFF0000) >> 16);
                // set color

            }

            switch (curOperationMode)
            {
                case ModeEnum.Draw:
                    if (curDraw != null)
                    {
                        curDraw.OnMouseDown(inkCanvas, e);
                    }
                    break;

                case ModeEnum.Select:
                    var selectionStartPoint = new Point?(e.GetPosition(inkCanvas));
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(inkCanvas);
                    if (adornerLayer != null)
                    {
                        SelectAdorner adorner = new SelectAdorner(inkCanvas, selectionStartPoint);
                        if (adorner != null)
                        {
                            adornerLayer.Add(adorner);
                        }
                    }
                    break;
            }


        }
        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(inkCanvas);
            if (point.X >= 0 && point.Y >= 0)
            {
                MouseLocationText = (int)point.X + "," + (int)point.Y + " пикселей";
            }
            else
            {
                MouseLocationText = "";
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (curDraw != null && inkCanvas.GetSelectedStrokes().Count == 0)
                {
                    isDraw = true;
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                    curDraw.OnMouseMove(inkCanvas, e);

                    if (curDraw.StrokeResult != null)
                    {
                        Rect rect = curDraw.StrokeResult.GetBounds();
                        FieldSizeText = (int)rect.Width + "," + (int)rect.Height + " пикселей";
                    }

                }
                else
                {
                    isDraw = false;
                }
            }

            if (curColorPickerMode)
            {
                StreamResourceInfo sri = Application.GetResourceStream(new Uri("/SvgPaint;component/Images/color_cursor.cur", UriKind.Relative));
                Cursor customCursor = new Cursor(sri.Stream);
                Mouse.OverrideCursor = customCursor;
            }

        }
        private void CanvasMouseLeave(object sender, MouseEventArgs e)
        {
            MouseLocationText = "";

            if (curColorPickerMode)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
        private void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (curDraw != null && isDraw)
            {
                if (curDraw.StrokeResult != null)
                {
                    var lstStrokes = new StrokeCollection() { curDraw.StrokeResult };
                    inkCanvas.Select(lstStrokes);
                    Strokes_Added(lstStrokes);
                    isDraw = false;
                }

            }
            else if (inkCanvas.EditingMode == InkCanvasEditingMode.Ink)
            {
                Strokes_Added(new StrokeCollection() { inkCanvas.Strokes[inkCanvas.Strokes.Count - 1] });
            }

        }

        #endregion

        #endregion

        #region win API

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("gdi32.dll")]
        private static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);


        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
