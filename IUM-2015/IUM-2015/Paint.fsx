open System.Windows.Forms
open System.Drawing

let f = new Form(Text="Paint")
f.TopMost <- true
f.Show()

// <NonSiFa>
// Grahpics Context
let g = Graphics.FromHwnd(nativeint(0x200B8))

// Immediate mode
g.FillRectangle(Brushes.Red, 0, 0, 200, 200)
g.FillEllipse(Brushes.Yellow, 0, 0, 200, 200)

let g1 = Graphics.FromHwnd(f.Handle)
g1.FillRectangle(Brushes.Red, 0, 0, 200, 200)
g1.FillEllipse(Brushes.Yellow, 0, 0, 200, 200)

//</NonSiFa>


f.Paint.Add(fun e ->
  let g = e.Graphics
  g.FillRectangle(Brushes.Red, 0, 0, 200, 200)
  g.FillEllipse(Brushes.Yellow, 0, 0, 200, 200)
)

f.Invalidate()


