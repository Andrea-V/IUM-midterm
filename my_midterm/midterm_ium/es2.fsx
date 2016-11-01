open System.Windows.Forms
open System.Drawing
open System.Collections.Generic

(* Cliccare col pulsante sinistro del mouse per impostare i punti del poligono.
 * Dopodichè, cliccare col destro all'interno del poligono per eseguire l'algoritmo
 * di flood-fill.
*)
type FillPolygon() as this =
  inherit UserControl()

  //vertici del poligono
  let mutable pts = Array.create 0 (new PointF())
  
  //punto iniziale da cui eseguire il flood fill
  let mutable (start:Point option) = None

  //buffer
  let mutable buffer : Bitmap = null

  //Implementazione algoritmo di flood fill (ricorsivo)
  let rec recFillPolygon (g:Graphics) (b:Bitmap) (start:Point) (c:Color)=
    if b.GetPixel(start.X,start.Y).ToArgb() = c.ToArgb() then
        g.FillRectangle(Brushes.Red,start.X,start.Y,1,1);
    
        let up   = new Point(start.X,start.Y-1)
        let down = new Point(start.X,start.Y+1)
        let left = new Point(start.X-1,start.Y)
        let right= new Point(start.X+1,start.Y)

        if up.Y>0 then
            recFillPolygon g b up c
        if down.Y<this.Height then 
            recFillPolygon g b down c
        if left.X>0 then
            recFillPolygon g b left c
        if right.X<this.Width then
            recFillPolygon g b right c

//Implementazione algoritmo di flood fill (iterativo)
  let itfillPolygon (g:Graphics) (b:Bitmap) (start:Point) (c:Color)=
    let mutable stack = new Stack<Point>()

    stack.Push(start)
    while stack.Count > 0 do
        let p = stack.Pop()
        if b.GetPixel(p.X,p.Y).ToArgb() = c.ToArgb() then
            g.FillRectangle(Brushes.Red,p.X,p.Y,1,1);

            let up   = new Point(p.X,p.Y-1)
            let down = new Point(p.X,p.Y+1)
            let left = new Point(p.X-1,p.Y)
            let right= new Point(p.X+1,p.Y)

            if up.Y>0 then
                stack.Push up
            if down.Y<this.Height then 
                stack.Push down
            if left.X>0 then
                stack.Push left
            if right.X<this.Width then
                stack.Push right
  
  //aggiorna il buffer, creando una nuova bitmap
  let updateBuffer () =
    if buffer = null || buffer.Width <> this.Width || buffer.Height <> this.Height then
      if buffer <> null then 
        buffer.Dispose()
      buffer <- new Bitmap(this.Width, this.Height)

  //double buffering
  do this.SetStyle(ControlStyles.OptimizedDoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)


  //EVENTI DEL MOUSE

  //se click col pulsante sx aggiungo un punto al poligono
  //se click col dx setto start
  override this.OnMouseUp e =
     if e.Button = MouseButtons.Left then
        match start with
        | Some p    ->  start <- None
                        pts   <- Array.create 1 (new PointF(single(e.X),single(e.Y)))
                        this.Invalidate()
        | None      ->  let p = new PointF(single(e.X), single(e.Y))
                        pts <- Array.append pts (Array.create 1 p)
                        this.Invalidate()
     else if e.Button = MouseButtons.Right then
        start <- Some(Point(e.X,e.Y))
        this.Invalidate()

  
  //eseguo il flood fill su una bitmap, poi disegno 
  override this.OnPaint e =
     updateBuffer()    
     let vg = e.Graphics
     let g  = Graphics.FromImage(buffer)
     
     //disegno sfondo
     g.FillRectangle(Brushes.AntiqueWhite,0,0,this.Width,this.Height)
     
     //mi servono almeno 3 punti per un poligono
     if pts.Length<3 then
        start <- None
     

     //se ho un punto iniziale eseguo flood-fill
     match start with
     | None     -> pts |> Array.iter (fun pt ->g.DrawEllipse(Pens.Black, pt.X-5.f, pt.Y-5.f, 10.f, 10.f))
                   if pts.Length > 1 then 
                       g.DrawPolygon(Pens.Blue,pts)

     | Some p   -> g.DrawPolygon(Pens.Blue,pts)
                   itfillPolygon g buffer p Color.AntiqueWhite
     
     //scrivo la bitmap sul buffer
     vg.DrawImage(buffer,0,0)


//
//  MAIN
//
let f = new Form(Text="Flood-Fill",TopMost=true)
let c = new FillPolygon(Dock=DockStyle.Fill)
f.Controls.Add(c)
f.Show()
