open System.Windows.Forms
open System.Drawing

let f = new Form(Text="Double buffering",TopMost=true)
f.Show()

type BouncingBall() =
  let mutable location = PointF()
  let mutable speed = SizeF(10.f,10.f) // px/s
  let mutable size = SizeF(25.f,25.f)
  let mutable lastT = System.DateTime.Now

  member this.Location with get() = location and set(v) = location <- v
  member this.Speed with get() = speed and set(v) = speed <- v
  member this.Size with get() = size and set(v) = size <- v
  member this.Bounds = new RectangleF(location, size)

  member this.UpdatePosition() =
    let t = System.DateTime.Now
    let dt = t - lastT
    let vx = speed.Width / 1000.f
    let vy = speed.Height / 1000.f
    let dx = vx * single(dt.TotalMilliseconds)
    let dy = vy * single(dt.TotalMilliseconds)
    location <- PointF(location.X + dx, location.Y + dy)
    lastT <- t

type BouncingBalls() as this =
  inherit UserControl()

  do this.SetStyle(ControlStyles.OptimizedDoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)

  let balls = new ResizeArray<BouncingBall>()
  let mutable buffer : Bitmap = null //new Bitmap(this.Width, this.Height)

  let updateBuffer () =
    if buffer = null || buffer.Width <> this.Width || buffer.Height <> this.Height then
      if buffer <> null then buffer.Dispose() // IMPORTANTE!!!!!!!!!!!!
      buffer <- new Bitmap(this.Width, this.Height)

  do balls.Add(new BouncingBall(Location=PointF(20.f, 30.f), Speed=SizeF(30.f, 30.f)))

  let updateBalls() =
    balls |> Seq.iter (fun b ->
      b.UpdatePosition()
      if b.Location.X < 0.f || b.Location.X + b.Size.Width > single(this.Width) then
        b.Speed <- SizeF(- b.Speed.Width, b.Speed.Height)
      if b.Location.Y < 0.f || b.Location.Y + b.Size.Height > single(this.Height) then
        b.Speed <- SizeF(b.Speed.Width, - b.Speed.Height)
    )

  let t = new Timer(Interval=30)

  do
    t.Tick.Add(fun _  ->
      updateBalls()
      this.Invalidate()
    )
    t.Start()
  
  override this.OnPaintBackground e = ()

  override this.OnPaint e =
    updateBuffer()

    let vg = e.Graphics

    let g = Graphics.FromImage(buffer)
    use bg = new SolidBrush(this.BackColor)
    g.FillRectangle(bg, 0, 0, buffer.Width, buffer.Height)

    balls |> Seq.iter (fun b ->
      let r = b.Bounds
      g.FillEllipse(Brushes.Red, r)
      g.DrawEllipse(Pens.DarkRed, r)
    )

    vg.DrawImage(buffer, 0, 0)

let bb = new BouncingBalls(Dock=DockStyle.Fill)
f.Controls.Add(bb)

