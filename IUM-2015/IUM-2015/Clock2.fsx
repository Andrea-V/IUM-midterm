open System.Windows.Forms
open System.Drawing

let f = new Form(Text="Clock", TopMost=true)
f.Show()

let cx, cy = 100.f, 100.f

f.Paint.Add(fun e ->
  let g = e.Graphics
  g.SmoothingMode <- Drawing2D.SmoothingMode.HighQuality

  let sx = single(f.ClientSize.Width)/(2.f*cx)
  let sy = single(f.ClientSize.Height)/(2.f*cy)
  g.ScaleTransform(sx, sy)
  g.TranslateTransform(cx, cy)
  //g.Transform <- new Drawing2D.Matrix(1.f, 0.f, 0.f, 1.f, cx, cy)
  g.RotateTransform(-90.f)
  let mutable s = g.Save()
  for i = 1 to 12 do
    g.DrawLine(Pens.Black, 90, 0, 100, 0)
    g.RotateTransform(30.f)
  g.Restore(s)
  s <- g.Save()
  let t = System.DateTime.Now
  g.RotateTransform(single((t.Hour % 12) * 30))
  g.DrawLine(Pens.Black, -5, 0, 60, 0)
  g.Restore(s)
  s <- g.Save()
  g.RotateTransform(single(t.Minute * 6))
  g.DrawLine(Pens.Black, -5, 0, 75, 0)
  g.Restore(s)
  s <- g.Save()
  g.RotateTransform(single(t.Second * 6))
  g.DrawLine(Pens.Red, -5, 0, 75, 0)
)

f.Resize.Add(fun _ ->
  f.Invalidate()
)

let timer = new Timer(Interval=1000)
timer.Tick.Add(fun _ ->
  f.Invalidate()
)
timer.Start()
