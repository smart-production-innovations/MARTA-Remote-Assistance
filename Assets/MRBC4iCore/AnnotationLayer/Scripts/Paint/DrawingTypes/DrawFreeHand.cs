using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Modification of the free unity 3d asset "Free Draw - Simple Drawing on Sprites/2D Textures"
// https://assetstore.unity.com/packages/tools/painting/free-draw-simple-drawing-on-sprites-2d-textures-113131

// 1. Attach this to a read/write enabled sprite image
// 2. Set the drawing_layers  to use in the raycast
// 3. Attach a 2D collider (like a Box Collider 2D) to this sprite
// 4. Hold down left mouse to draw on this texture!
public class DrawFreeHand : DrawingCoordinatesBase<DrawFreeHand>
{
    private class DrawingLine
    {
        public Vector2 StartPoint { get; set; }
        public Vector2 EndPoint { get; set; }

        private float distance = 0;
        public float Distance
        {
            get
            {
                if (distance == 0)
                    distance = Vector2.Distance(StartPoint, EndPoint);
                return distance;
            }
        }

        private Vector2 direction = Vector2.zero;
        public Vector2 Direction
        {
            get
            {
                if (direction == Vector2.zero)
                    direction  = (StartPoint - EndPoint).normalized;
                return direction;
            }
        }

        private Vector2[] lineDot = null;
        public Vector2[] LineDot
        {
            get
            {
                if (lineDot == null)
                {
                    var pixelCount = (int)Math.Ceiling(Distance);
                    lineDot = new Vector2[pixelCount];

                    // Calculate how many times we should interpolate between StartPoint and EndPoint based on the amount of time that has passed since the last update
                    float lerp_steps = 1 / Distance;

                    for (int i = 0; i < pixelCount; i++)
                    {
                        var lerp = i * lerp_steps;
                        var cur_position = Vector2.Lerp(StartPoint, EndPoint, lerp);
                        lineDot[i] = cur_position;
                    }
                }
                return lineDot;
            }
        }


        public int Width { get; set; }
        public Color Color { get; set; }
        public float Alpha
        {
            get { return Color.a; }
            set
            {
                var color = Color;
                color.a = value;
                Color = color;
                hasChanged = true;
            }
        }
        public float Time { get; set; }

        private bool hasChanged;

        public bool HasChanged
        {
            get
            {
                var changed = hasChanged;
                hasChanged = false;
                return changed;
            }
        }

        private int drawCount = 0;
        private int lastDrawingFrame = -10;

        private int previousDrawingAreaWidth = -1;
        private List<Vector2Int> linePixel = null;

        private void preCalculateDrawingPixel(int drawingAreaWidth, int drawingAreaHeight)
        {
            var pixelCountMatrix = new int[drawingAreaWidth, drawingAreaHeight];
            linePixel = new List<Vector2Int>();
            foreach (var dotCenter in LineDot)
            {
                // Figure out how many pixels we need to colour in each direction (x and y)
                int center_x = (int)dotCenter.x;
                int center_y = (int)dotCenter.y;

                for (int x = center_x - Width; x <= center_x + Width; x++)
                {
                    // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
                    if (x >= (int)drawingAreaWidth
                        || x < 0)
                        continue;

                    for (int y = center_y - Width; y <= center_y + Width; y++)
                    {
                        pixelCountMatrix[x, y] = pixelCountMatrix[x, y] + 1;
                        if (pixelCountMatrix[x, y] == 1)
                        {
                            var pixelPos = new Vector2Int(x, y);
                            linePixel.Add(pixelPos);
                        }
                    }
                }
            }
        }

        public Color32[] colourPixels (Color32[] pixelArray, int drawingAreaWidth, int drawingAreaHeight, int[,] pixelCountMatrix = null)
        {
            if (previousDrawingAreaWidth != drawingAreaWidth || linePixel == null)
            {
                preCalculateDrawingPixel(drawingAreaWidth, drawingAreaHeight);
                previousDrawingAreaWidth = drawingAreaWidth;
            }

            foreach (var pixel in linePixel)
            {
                bool redrawPixel = (lastDrawingFrame < drawCount - 1 || Alpha == 0);
                if (pixelCountMatrix != null)
                {
                    pixelCountMatrix[pixel.x, pixel.y] = pixelCountMatrix[pixel.x, pixel.y] + 1;
                    if (pixelCountMatrix[pixel.x, pixel.y] > 1) redrawPixel = false;
                }

                if (redrawPixel)
                {
                    // Need to transform x and y coordinates to flat coordinates of array
                    int array_pos = (int)(pixel.y * drawingAreaWidth + pixel.x);

                    // Check if this is a valid position
                    if (array_pos < pixelArray.Length && array_pos >= 0)
                        pixelArray[array_pos] = Color;
                }
            }

            if (lastDrawingFrame < drawCount - 1 || Alpha == 0)
                lastDrawingFrame = drawCount;
            drawCount++;

            return pixelArray;
        }



        public DrawingLine (Vector2 start_point, Vector2 end_point, int width, Color color, float createTime)
        {
            this.StartPoint = start_point;
            this.EndPoint = end_point;
            this.Width = width;
            this.Color = color;
            this.Time = createTime;
            this.hasChanged = true;
        }
    }

    // Change these to change the default drawing settings
    // PEN WIDTH (actually, it's a radius, in pixels)
    public int Pen_Width = 3;

    public Image readWriteEnabledImage;
    public Image drawingImage;

    protected override void OnDestroy()
    {
        ResolutionManager.OnPResolutionChanged -= CreateDrawableTexture;

        base.OnDestroy();
    }

    public void CreateDrawableTexture(int width, int height)
    {
        CreateDrawableTexture(width, height, true);
    }

    public void CreateDrawableTexture(int width, int height, bool initDrawingCanvas)
    {
        int drawing_width = StatusProperties.Values.DrawingResizeFactor * width;
        int drawing_height = StatusProperties.Values.DrawingResizeFactor * height;

        drawable_texture = new Texture2D(drawing_width, drawing_height);

        Sprite sprite = Sprite.Create(drawable_texture, new Rect(0, 0, drawing_width, drawing_height), new Vector2(0.5f, 0.5f), 100);
        readWriteEnabledImage.sprite = sprite;
        drawingImage.sprite = sprite;

        if (initDrawingCanvas)  
            InitDrawing();
    }

    public delegate void Brush_Function();// Vector2 world_position);
    // This is the function called when a left click happens
    // Pass in your own custom one to change the brush type
    // Set the default function in the Awake method
    public Brush_Function current_brush;
    public Brush_Function current_brush_hide_over_time;

    public delegate void Brush_Function_Position(Vector2 world_position);
    // This is the function called when a left click happens
    // Pass in your own custom one to change the brush type
    // Set the default function in the Awake method
    public Brush_Function_Position current_position_brush;


    // The colour the canvas is reset to each time
    public Color Reset_Colour = new Color(255f, 255f, 255f, 0f); //new Color(0, 0, 0, 0);  // By default, reset the canvas to be transparent


    // MUST HAVE READ/WRITE enabled set in the file editor of Unity
    Texture2D drawable_texture;

    Vector2 previous_drag_position;
    Color[] clean_colours_array;
    Color transparent;
    Color32[] cur_colors;
    bool mouse_was_previously_held_down = false;
    bool no_drawing_on_current_drag = false;

    //////////////////////////////////////////////////////////////////////////////
    // BRUSH TYPES. Implement your own here


    private List<DrawingLine> drawingQueue = new List<DrawingLine>();
    private List<DrawingLine> visibleLines = new List<DrawingLine>();
    public void AddToQueue(Vector2 pixel_pos)
    {
        if (previous_drag_position != Vector2.zero && previous_drag_position != pixel_pos)
        {
            var newLine = new DrawingLine(previous_drag_position, pixel_pos, Pen_Width, DrawingColor, Time.time);
            drawingQueue.Add(newLine);
            visibleLines.Insert(0, newLine);
        }
    }

    public override void AddMousePositionToQueue()
    {
        // Is the user holding down the left mouse button?
        bool mouse_held_down = Input.GetMouseButton(0);
        if (mouse_held_down && !no_drawing_on_current_drag)
        {
            bool insideDrawingArea = false;
            Vector2 pixel_pos = getPositionOnDrawingArea(out insideDrawingArea);

            if (insideDrawingArea)
            {
                AddToQueue(pixel_pos);
                previous_drag_position = pixel_pos;
            }
            else
            {
                // We're not over our destination texture
                previous_drag_position = Vector2.zero;
                if (!mouse_was_previously_held_down)
                {
                    // This is a new drag where the user is left clicking off the canvas
                    // Ensure no drawing happens until a new drag is started
                    no_drawing_on_current_drag = true;
                }
            }
        }
        // Mouse is released
        else if (!mouse_held_down)
        {
            previous_drag_position = Vector2.zero;
            no_drawing_on_current_drag = false;
        }
        mouse_was_previously_held_down = mouse_held_down;
    }

    private bool visibleLinesHasChanged()
    {
        bool hasChanged = false;
        foreach (var item in visibleLines)
        {
            hasChanged = hasChanged || item.HasChanged;
        }
        return hasChanged;
    }

    // When you want to make your own type of brush effects,
    // Copy, paste and rename this function.
    // Go through each step
    public void BrushTemplate(Vector2 pixel_pos)
    {
        // 2. Make sure our variable for pixel array is updated in  this frame
        //cur_colors = drawable_texture.GetPixels32();

        ////////////////////////////////////////////////////////////////
        // FILL IN CODE BELOW HERE

        // Do we care about the user left clicking and dragging?
        // If you don't, simply set the below if statement to be:
        // if (true)
        if (previous_drag_position == Vector2.zero)
        {
            // THIS IS THE FIRST CLICK
            // FILL IN WHATEVER YOU WANT TO DO HERE
            // Maybe mark multiple pixels to colour?
            MarkPixelsToColour(pixel_pos, Pen_Width, DrawingColor);
        }
        else
        {
            // THE USER IS DRAGGING
            // Should we do stuff between the rpevious mouse position and the current one?
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, DrawingColor);
        }
        ////////////////////////////////////////////////////////////////

        // 3. Actually apply the changes we marked earlier
        // Done here to be more efficient
        ApplyMarkedPixelChanges();

        // 4. If dragging, update where we were previously
        previous_drag_position = pixel_pos;
    }


    // Default brush type. Has width and colour.
    // Pass in a point in WORLD coordinates
    // Changes the surrounding pixels of the world_point to the static pen_colour
    public void PenBrush()
    {
        int drawingAreaWidth = (int)drawable_sprite.rect.width;
        int drawingAreaHeight = (int)drawable_sprite.rect.height;
        if (drawingQueue.Count > 0)
        {
            //cur_colors = drawable_texture.GetPixels32();

            var deleteList = new List<DrawingLine>();
            foreach (var item in drawingQueue)
            {
                // Colour in a line from where we were on the last update call
                item.colourPixels(cur_colors, drawingAreaWidth, drawingAreaHeight);
                deleteList.Add(item);
            }

            foreach (var item in deleteList)
            {
                drawingQueue.Remove(item);
            }
            deleteList.Clear();

            ApplyMarkedPixelChanges();
        }
    }

    // Default brush type. Has width and colour.
    // Pass in a point in WORLD coordinates
    // Changes the surrounding pixels of the world_point to the static pen_colour
    public void PenBrushHideOverTime()
    {
        int drawingAreaWidth = (int)drawable_sprite.rect.width;
        int drawingAreaHeight = (int)drawable_sprite.rect.height;
        var pixelCountMatrix = new int[drawingAreaWidth, drawingAreaHeight];
        if (visibleLinesHasChanged())
        {
            var deleteList = new List<DrawingLine>();
            foreach (var item in visibleLines)
            {
                // Colour in a line from where we were on the last update call
                item.colourPixels(cur_colors, drawingAreaWidth, drawingAreaHeight, pixelCountMatrix);

                if (item.Alpha == 0)
                {
                    deleteList.Add(item);
                }
            }

            foreach (var item in deleteList)
            {
                visibleLines.Remove(item);
            }
            deleteList.Clear();

            ApplyMarkedPixelChanges();
        }
    }

    public void SaveFrame(int frameNo, Texture2D tex)
    {
        StartCoroutine(AsyncSaveFrame(frameNo, tex));
    }

    IEnumerator AsyncSaveFrame(int frameNo, Texture2D tex)
    {
        var video = SearchHelper.FindSceneObjectOfType<VideoStream>();
        if (video)
        {
            byte[] bytes = tex.EncodeToPNG();

            var imagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var dir = Path.Combine(imagePath, "MRBC4i", RemoteCallManager.Instance.UniqueID);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, frameNo.ToString("0000") + "_" + (int)(Time.time * 10000) + ".png");
            File.WriteAllBytes(path, bytes);
        }
        yield return null;
    }

    // Default brush type. Has width and colour.
    // Pass in a point in WORLD coordinates
    // Changes the surrounding pixels of the world_point to the static pen_colour
    public void PenBrush(Vector2 pixel_pos)
    {
        //cur_colors = drawable_texture.GetPixels32();

        if (previous_drag_position == Vector2.zero)
        {
            // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
            MarkPixelsToColour(pixel_pos, Pen_Width, DrawingColor);
        }
        else
        {
            // Colour in a line from where we were on the last update call
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, DrawingColor);
        }
        ApplyMarkedPixelChanges();

        previous_drag_position = pixel_pos;
    }


    // Helper method used by UI to set what brush the user wants
    // Create a new one for any new brushes you implement
    public void SetPenBrush()
    {
        // PenBrush is the NAME of the method we want to set as our current brush
        current_brush = PenBrush;
        current_position_brush = PenBrush;
        current_brush_hide_over_time = PenBrushHideOverTime;
    }
    //////////////////////////////////////////////////////////////////////////////

    // This is where the magic happens.
    // Detects when user is left clicking, which then call the appropriate function
    public override void UpdateDrawing()
    {
        if (drawingQueue.Count  > 0 && !hideDrawingOverTime)
        {
            current_brush();
        }
        else if (hideDrawingOverTime)
        {
            drawingQueue.Clear();
        }
    }

    private void UpdateDrawingOld()
    {
        // Is the user holding down the left mouse button?
        bool mouse_held_down = Input.GetMouseButton(0);
        if (mouse_held_down && !no_drawing_on_current_drag)
        {
            bool insideDrawingArea = false;
            Vector2 pixel_pos = getPositionOnDrawingArea(out insideDrawingArea);

            if (insideDrawingArea)
            {
                current_position_brush(pixel_pos);
            }
            else
            {
                // We're not over our destination texture
                previous_drag_position = Vector2.zero;
                if (!mouse_was_previously_held_down)
                {
                    // This is a new drag where the user is left clicking off the canvas
                    // Ensure no drawing happens until a new drag is started
                    no_drawing_on_current_drag = true;
                }
            }
        }
        // Mouse is released
        else if (!mouse_held_down)
        {
            previous_drag_position = Vector2.zero;
            no_drawing_on_current_drag = false;
        }
        mouse_was_previously_held_down = mouse_held_down;
    }

    /// <summary>
    /// update edit activity for selected drawing layer. abstract function must be implemented.
    /// </summary>
    public override void EditDrawing()
    {
    }

    // Set the colour of pixels in a straight line from start_point all the way to end_point, to ensure everything inbetween is coloured
    public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
    {
        // Get the distance from start to finish
        float distance = Vector2.Distance(start_point, end_point);
        Vector2 direction = (start_point - end_point).normalized;

        Vector2 cur_position = start_point;

        // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }
    }

    public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        // Figure out how many pixels we need to colour in each direction (x and y)
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;
        int extra_radius = Mathf.Min(0, pen_thickness - 2);

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
            if (x >= (int)drawable_sprite.rect.width
                || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                MarkPixelToChange(x, y, color_of_pen);
            }
        }
    }

    public void MarkPixelToChange(int x, int y, Color color)
    {
        // Need to transform x and y coordinates to flat coordinates of array
        int array_pos = y * (int)drawable_sprite.rect.width + x;

        // Check if this is a valid position
        if (array_pos > cur_colors.Length || array_pos < 0)
            return;

        cur_colors[array_pos] = color;
    }

    /// <summary>
    /// let the drawings disappear over the time
    /// </summary>
    public override void HideDrawingOverTime()
    {
        foreach (var item in visibleLines)
        {
            if (item.Alpha > 0)
            {
                var delta = transparencyDeltaValue / 255f;
                var threshold = visibilityThreshold / 255f;
                //delta *= 3;
                var alpha = item.Alpha - delta;
                if (alpha < threshold) alpha = 0;
                if (item.Alpha > alpha)
                    item.Alpha = alpha;
            }

        }
    }

    private void HideDrawingOverTime_old()
    {
        if (cur_colors != null)
        {
            bool hasColorChanges = false;
            for (int i = 0; i < cur_colors.Length; i++)
            {
                if (cur_colors[i].a > 0)
                {
                    hasColorChanges = true;

                    var alpha = cur_colors[i].a - transparencyDeltaValue;
                    if (cur_colors[i].a < transparencyDeltaValue) alpha = 0;

                    cur_colors[i].a = (byte)alpha;
                }
            }
            if (hasColorChanges) ApplyMarkedPixelChanges();
        }
    }

    public override void UpdateDrawingAlpha()
    {
        if (visibleLines.Count > 0)
        {
            current_brush_hide_over_time();
        }
    }

    /// <summary>
    /// Apply the changes we marked earlier
    /// </summary>
    public void ApplyMarkedPixelChanges()
    {
        drawable_texture.SetPixels32(cur_colors);
        drawable_texture.Apply();
    }


    // Directly colours pixels. This method is slower than using MarkPixelsToColour then using ApplyMarkedPixelChanges
    // SetPixels32 is far faster than SetPixel
    // Colours both the center pixel, and a number of pixels around the center pixel based on pen_thickness (pen radius)
    public void ColourPixels(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        // Figure out how many pixels we need to colour in each direction (x and y)
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;
        int extra_radius = Mathf.Min(0, pen_thickness - 2);

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                drawable_texture.SetPixel(x, y, color_of_pen);
            }
        }

        drawable_texture.Apply();
    }



    // Changes every pixel to be the reset colour
    override public void ResetCanvas()
    {
        if (drawable_texture != null && clean_colours_array != null)
        {
            drawable_texture.SetPixels(clean_colours_array);
            drawable_texture.Apply();
            cur_colors = drawable_texture.GetPixels32();
            //cur_colors = null;
            visibleLines.Clear();
            drawingQueue.Clear();
        }
    }

    /// <summary>
    /// is user coloring the art board or remove color from it
    /// </summary>
    /// <returns></returns>
    public bool IsEraserActive()
    {
        return (DrawingColor == Reset_Colour);
    }

    override protected void Awake()
    {
        int width = ResolutionManager.Instance.pWidth;
        int height = ResolutionManager.Instance.pHeight;
        if (width > 0 && height > 0)
        {
            CreateDrawableTexture(width, height, false);
        }
        ResolutionManager.OnPResolutionChanged += CreateDrawableTexture;
        base.Awake();
    }

    public override void InitDrawing()
    {
        base.InitDrawing();

        // DEFAULT BRUSH SET HERE
        SetPenBrush();
        cur_colors = drawable_texture.GetPixels32();

        if (drawable_sprite != null)
        {
            drawable_texture = drawable_sprite.texture;

            // Initialize clean pixels to use
            clean_colours_array = new Color[(int)drawable_sprite.rect.width * (int)drawable_sprite.rect.height];
            for (int x = 0; x < clean_colours_array.Length; x++)
                clean_colours_array[x] = Reset_Colour;

            //Reset needs drawable_texture, therefore cal again here
            if (Reset_Canvas_On_Play)
                ResetCanvas();
        }

        showARHelper = false;
    }

    override public void OnEnable()
    {
        base.OnEnable();

        previous_drag_position = Vector2.zero;
        no_drawing_on_current_drag = false;
        mouse_was_previously_held_down = false;

        if (loadContentOnEnabel) loadDrawing();

        showARHelper = false;
    }

    private Texture2D loadTexture;
    private Vector2 loadPivot;
    private bool loadContentOnEnabel = false;

    /// <summary>
    /// Reload previous drawing to active drawing panel. This step allow the user to modify previous drawings.
    /// </summary>
    /// <param name="tex">texture which should be reloaded</param>
    /// <param name="pivot">pivot point of the previous drawing</param>
    public void loadDrawing(Texture2D tex, Vector2 pivot)
    {
        this.loadContentOnEnabel = true;
        this.loadTexture = tex;
        this.loadPivot = pivot;
        if (gameObject.activeInHierarchy && enabled) loadDrawing();
    }

    /// <summary>
    /// Reload previous drawing to active drawing panel. This step allow the user to modify previous drawings.
    /// </summary>
    public void loadDrawing()
    {

        var pos = Vector2.zero;

        if (StatusProperties.Values.CalculationMode == CalculationMode.ClickPointPlane)
        {
            Vector2 loadSize = new Vector2(loadTexture.width, loadTexture.height);
            Vector2 targetSize = new Vector2(drawable_texture.width, drawable_texture.height);
            Vector2 targetPivot = targetSize / 2;
            pos = targetPivot - (loadPivot * loadSize);
        }

        drawable_texture.SetPixels((int)pos.x, (int)pos.y, loadTexture.width, loadTexture.height, loadTexture.GetPixels());
        drawable_texture.Apply();

        loadContentOnEnabel = false;
    }

    protected override DrawingLayer[] GetDrawingLayers()
    {
        var list = base.GetDrawingLayers();
        list = list.Where(x => x.LayerType == LayerType.FreeHand).ToArray();
        return list;
    }
}