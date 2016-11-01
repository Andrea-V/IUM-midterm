#load "LWCs.fsx"

open System.Windows.Forms
open System.Drawing
open LWCs

//bottoni della navbar
type NavBut =
| Up = 0
| Right = 1
| Left = 2
| Down = 3
| Clock = 4
| AClock= 5
| ZoomIn= 6
| ZoomOut= 7

//bottoni delle primitive
type PrimBut =
| Line = 0
| Curve = 1
| Bezier = 2

//bottoni dell'editor
type ColBut =
| Red    = 0
| Blue   = 1
| Yellow = 2
| Green  = 3
| Purple = 4
| Orange = 5
| White  = 6
| Gray   = 7
| Black  = 8

//bottoni dello spessore
type ThickBut =
| Thin    = 0
| Normal  = 1
| Thick   = 2

//bottoni della tensione
type TensBut =
| More  = 0
| Less  = 1


//
//  BOTTONE QUADRATO
//
type SquareButton() as this =
  inherit LWC()

  let clickevt = new Event<System.EventArgs>()
  let downevt = new Event<MouseEventArgs>()
  let upevt = new Event<MouseEventArgs>()
  let moveevt = new Event<MouseEventArgs>()
  
  do this.Size <- SizeF(64.f, 32.f)

  let mutable color= Brushes.Gray
  let mutable text = ""

  member this.Click = clickevt.Publish
  member this.MouseDown = downevt.Publish
  member this.MouseUp = upevt.Publish
  member this.MouseMove = moveevt.Publish

  member this.Text
    with get() = text
    and set(v) = text <- v; this.Invalidate()

  member this.Color
    with get() = color
    and set(v) = color <- v; this.Invalidate()
    
  override this.OnMouseUp e = upevt.Trigger(e); clickevt.Trigger(new System.EventArgs())
  override this.OnMouseMove e = moveevt.Trigger(e)
  override this.OnMouseDown e = downevt.Trigger(e)


  //on paint
  override this.OnPaint e =
    let g = e.Graphics

    g.FillRectangle(color, new Rectangle(0,0, int(this.Size.Width),int(this.Size.Height)))
    let sz = g.MeasureString(text, this.Parent.Font)
    g.DrawString(text, this.Parent.Font, Brushes.Black, PointF((this.Size.Width - sz.Width) / 2.f, (this.Size.Height - sz.Height) / 2.f))


//
//BOTTONE CIRCOLARE
//
type CircleButton() as this =
  inherit LWC()

  let clickevt = new Event<System.EventArgs>()
  let downevt = new Event<MouseEventArgs>()
  let upevt = new Event<MouseEventArgs>()
  let moveevt = new Event<MouseEventArgs>()

  do this.Size <- SizeF(50.f,50.f)

  let mutable text = ""
  let mutable color= Brushes.LightGreen

  member this.Click = clickevt.Publish
  member this.MouseDown = downevt.Publish
  member this.MouseUp = upevt.Publish
  member this.MouseMove = moveevt.Publish

  member this.Text
    with get() = text
    and set(v) = text <- v; this.Invalidate()

  member this.Color
    with get() = color
    and set(v) = color <- v; this.Invalidate()
  
  override this.OnMouseUp e = upevt.Trigger(e); clickevt.Trigger(new System.EventArgs())
  override this.OnMouseMove e = moveevt.Trigger(e)
  override this.OnMouseDown e = downevt.Trigger(e)


  //on paint
  override this.OnPaint e =
    let g = e.Graphics
    g.FillEllipse(color, new Rectangle(0,0, int(this.Size.Width),int(this.Size.Height)))
    let sz = g.MeasureString(text, this.Parent.Font)
    g.DrawString(text, this.Parent.Font, Brushes.Black, PointF((this.Size.Width - sz.Width) / 2.f, (this.Size.Height - sz.Height) / 2.f))

// 
// bottone per lo spessore
//
type ThicknessButton() as this =
  inherit LWC()

  let clickevt = new Event<System.EventArgs>()
  let downevt = new Event<MouseEventArgs>()
  let upevt = new Event<MouseEventArgs>()
  let moveevt = new Event<MouseEventArgs>()

  do this.Size <- SizeF(150.f,20.f)

  let mutable text = ""
  let mutable color = Brushes.Red
  let mutable thickness = ThickBut.Normal

  let getConstant t =
    match t with
    | ThickBut.Thin    -> 2.f/5.f
    | ThickBut.Normal  -> 1.f/5.f
    | ThickBut.Thick   -> 0.5f/5.f
    | _                -> 0.f

  member this.Click = clickevt.Publish
  member this.MouseDown = downevt.Publish
  member this.MouseUp = upevt.Publish
  member this.MouseMove = moveevt.Publish

  member this.Thickness
    with get() = thickness
    and set(v) = thickness <- v; this.Invalidate()

  member this.Text
    with get() = text
    and set(v) = text <- v; this.Invalidate()
    
  override this.OnMouseUp e = upevt.Trigger(e); clickevt.Trigger(new System.EventArgs())
  override this.OnMouseMove e = moveevt.Trigger(e)
  override this.OnMouseDown e = downevt.Trigger(e)


  //on paint
  override this.OnPaint e =
    let g = e.Graphics
    let width = this.Size.Width
    let height  = this.Size.Height
    g.FillRectangle(Brushes.Gray, new Rectangle(0,0,int(width),int(height)))
    g.FillRectangle(Brushes.Black, new Rectangle(2,2,int(width-4.f),int(height-4.f)))
    
    let k = getConstant this.Thickness
    g.FillRectangle(Brushes.White, new Rectangle(2,2,int(width-4.f),int((height-4.f)*k)))
    g.FillRectangle(Brushes.White, new Rectangle(2,int((height)*(1.f-k)),int(width-4.f),int((height)-((height)*(1.f-k)))))

    let sz = g.MeasureString(text, this.Parent.Font)
    g.DrawString(text, this.Parent.Font, Brushes.Black, PointF((this.Size.Width - sz.Width) / 2.f, (this.Size.Height - sz.Height) / 2.f))


//
// bottone dei colori
//
type ColorButton() as this =
  inherit LWC()

  let clickevt = new Event<System.EventArgs>()
  let downevt = new Event<MouseEventArgs>()
  let upevt = new Event<MouseEventArgs>()
  let moveevt = new Event<MouseEventArgs>()

  do this.Size <- SizeF(50.f, 30.f)

  let mutable text = ""
  let mutable color = Brushes.Red

  member this.Click = clickevt.Publish
  member this.MouseDown = downevt.Publish
  member this.MouseUp = upevt.Publish
  member this.MouseMove = moveevt.Publish

  member this.Color
    with get() = color
    and set(v) = color <- v; this.Invalidate()

  member this.Text
    with get() = text
    and set(v) = text <- v; this.Invalidate()
    
  override this.OnMouseUp e = upevt.Trigger(e); clickevt.Trigger(new System.EventArgs())
  override this.OnMouseMove e = moveevt.Trigger(e)
  override this.OnMouseDown e = downevt.Trigger(e)


  //on paint
  override this.OnPaint e =
    let g = e.Graphics
    g.FillRectangle(color, new Rectangle(0,0, int(this.Size.Width),int(this.Size.Height)))
    let sz = g.MeasureString(text, this.Parent.Font)
    g.DrawString(text, this.Parent.Font, Brushes.Black, PointF((this.Size.Width - sz.Width) / 2.f, (this.Size.Height - sz.Height) / 2.f))