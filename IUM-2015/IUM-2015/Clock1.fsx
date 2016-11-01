open System.Windows.Forms
open System.Drawing

let f = new Form(Text="Clock", TopMost=true)
f.Show()

let cx, cy = 100, 100

let drawTick (g:Graphics) p r l a =
   let a1 = (a / 180.) * System.Math.PI
   let x1 = int(r * cos(a1))
   let y1 = int(r * sin(a1))
   let x2 = int((r + l) * cos(a1))
   let y2 = int((r + l) * sin(a1))
   g.DrawLine(p, cx + x1, cy + y1, cx + x2, cy + y2)

let drawLine (g:Graphics) p l a =
   drawTick g p 0. 10. (a + 180.)
   drawTick g p 0. l a

let drawTime (g:Graphics) (t:System.DateTime) =
   let ah = float(t.Hour % 12) * 30. - 90.
   let am = float(t.Minute) * 6. - 90.
   let asec = float(t.Second) * 6. - 90.
   use hp = new Pen(Color.Black)
   use mp = new Pen(Color.Black, Width=2.f)
   hp.Width <- 5.f
   drawLine g hp 60. ah
   drawLine g mp 75. am
   drawLine g Pens.Red 75. asec

f.Paint.Add(fun e ->
  let g = e.Graphics
  g.SmoothingMode <- Drawing2D.SmoothingMode.HighQuality
  for a in 0. .. 30. .. 360. do
    drawTick g Pens.Black 85. 15. a
  drawTime g (System.DateTime.Now)
)

let timer = new Timer(Interval=1000)
timer.Tick.Add(fun _ ->
  f.Invalidate()
)
timer.Start()
