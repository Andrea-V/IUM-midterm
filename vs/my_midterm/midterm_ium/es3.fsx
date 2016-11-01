#load "LWCs.fsx"
#load "Buttons.fsx"
#load "Primitives.fsx"

open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D
open LWCs
open Buttons
open Primitives
open System.Collections.Generic

//+-----------------+
//|     EDITOR      |
//+-----------------+
//Cliccare nella zone del foglio col pulsante sinistro per creare nuovi punti, l'editor
//disegnerà la primitiva non appena ci saranno abbastanza punti (il numero di punti minimo
//varia da primitiva a primitiva).
//Cliccare sui punti gia' creati col pulsante destro per eliminarli, in questo caso la primitiva
//corrispondente verraà riadattata ai nuovi punti, se possibile (i.e.: se i punti rimanenti sono in
//numero sufficiente).
//Cliccare col pulsante destro sulla zone del foglio (NON sui punti già esistenti) per terminare
//l'editing della primitiva corrente e iniziare una nuova primitiva
// 
// NB: tutti i bottoni di editing modificano la primitiva corrente. Una volta passati ad una nuova
// primitiva, non sara' piu' possibile modificarla.
type Editor() as this =
  inherit LWContainer()
  
  //dimensione delle handles
  let handleSize = 5.f


  //contiene il punto selezionato al momento
  let mutable selected   = None

  //offset del drag&drop
  let mutable offsetDrag = PointF()
  
  //imposta la creazione di una nuova primitiva
  let mutable newPrim    = true
  
  //segnala la presenza di un'azione di drag&drop
  let mutable dragAndDrop = false

  //array dei punti
  let mutable pts   = [||]
  
  //array delle primitive
  let mutable prims = Array.create 0 (new Primitive())
  
  //direzione di scroll
  let mutable scrollDir = NavBut.Up
  
  //primitiva correntemente selezionata
  let mutable lineType  = PrimBut.Line 
  
  //timer usato per l'autoscroll
  let scrollTimer = new Timer(Interval=100)
 
 
  //bottoni della barra di navigazione
  let navbar = [| 
    new CircleButton(Text="UP"   ,Location=PointF(50.f, 0.f));
    new CircleButton(Text="RIGHT",Location=PointF(100.f, 50.f));
    new CircleButton(Text="LEFT" ,Location=PointF(0.f, 50.f));
    new CircleButton(Text="DOWN" ,Location=PointF(50.f, 100.f));
    new CircleButton(Text="(" ,Location=PointF(150.f, 50.f));
    new CircleButton(Text=")" ,Location=PointF(200.f, 50.f));
    new CircleButton(Text="+" ,Location=PointF(150.f, 0.f));
    new CircleButton(Text="-" ,Location=PointF(200.f, 0.f));
  |]
  
  //bottoni delle primitive
  let primitives = [| 
    new SquareButton(Text="Line" ,Location=PointF(5.f,  170.f));
    new SquareButton(Text="Curve" ,Location=PointF(5.f, 220.f));
    new SquareButton(Text="Bezier" ,Location=PointF(5.f,270.f));
  |]

  //bottoni dei colori
  let palette = [| 
    new ColorButton(Text="" ,Location=PointF(5.f,320.f),Color=Brushes.Red);
    new ColorButton(Text="" ,Location=PointF(55.f,320.f),Color=Brushes.Blue);
    new ColorButton(Text="" ,Location=PointF(105.f,320.f),Color=Brushes.Yellow);
    new ColorButton(Text="" ,Location=PointF(5.f,350.f),Color=Brushes.Green);
    new ColorButton(Text="" ,Location=PointF(55.f,350.f),Color=Brushes.Purple);
    new ColorButton(Text="" ,Location=PointF(105.f,350.f),Color=Brushes.Orange);
    new ColorButton(Text="" ,Location=PointF(5.f,380.f),Color=Brushes.White);
    new ColorButton(Text="" ,Location=PointF(55.f,380.f),Color=Brushes.Gray);
    new ColorButton(Text="" ,Location=PointF(105.f,380.f),Color=Brushes.Black);
  |]

  //bottoni dello spessore
  let thicknesses = [| 
    new ThicknessButton(Text="" ,Location=PointF(5.f,430.f),Thickness=ThickBut.Thin);
    new ThicknessButton(Text="" ,Location=PointF(5.f,460.f),Thickness=ThickBut.Normal);
    new ThicknessButton(Text="" ,Location=PointF(5.f,490.f),Thickness=ThickBut.Thick);
  |]

  //bottoni della tensione
  let tensions = [|
    new CircleButton(Text="+T",Location=PointF(75.f, 220.f),Size=SizeF(30.f,30.f),Color=Brushes.DarkKhaki);
    new CircleButton(Text="-T",Location=PointF(105.f, 220.f),Size=SizeF(30.f,30.f),Color=Brushes.DarkKhaki);
  |]

  //matrici di trasformazione 
  let mutable w2v = new Drawing2D.Matrix()
  let mutable v2w = new Drawing2D.Matrix()

  //traslazione
  let translateW (tx, ty) =
    w2v.Translate(tx, ty)
    v2w.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)
  
  //traslazione nel punto (x,y)
  let translate (x, y) =
    let t = [| PointF(0.f, 0.f); PointF(x, y) |]
    v2w.TransformPoints(t)
    translateW(t.[1].X - t.[0].X, t.[1].Y - t.[0].Y)

  //rotazione
  let rotateW a =
    w2v.Rotate a
    v2w.Rotate(-a, Drawing2D.MatrixOrder.Append)
  
  //rotazione + traslazione
  let rotateAtW p a =
    w2v.RotateAt(a, p)
    v2w.RotateAt(-a, p, Drawing2D.MatrixOrder.Append)
  
  //scalatura (zoom)
  let scaleW (sx, sy) =
    w2v.Scale(sx, sy)
    v2w.Scale(1.f/sx, 1.f/sy, Drawing2D.MatrixOrder.Append)
  
  //trasforma le coordinate p in base ad m
  let transformP (m:Drawing2D.Matrix) (p:Point) =
    let a = [| PointF(single p.X, single p.Y) |]
    m.TransformPoints(a)
    a.[0]

  //hit test dell'handle delle linee
  let handleHitTest (p:PointF) (h:PointF) =
    let x = p.X - h.X
    let y = p.Y - h.Y
    x * x + y * y < handleSize * handleSize

  //hit test dei bottoni dell'editor
  let buttonHitTest (p:PointF) =
    let nav  = Array.fold(fun (s0:bool) (b:CircleButton) -> s0 || b.HitTest p) false navbar
    let prim = Array.fold(fun (s0:bool) (b:SquareButton) -> s0 || b.HitTest p) false primitives
    let edit = Array.fold(fun (s0:bool) (b:ColorButton) -> s0 || b.HitTest p) false palette
    let thick= Array.fold(fun (s0:bool) (b:ThicknessButton) -> s0 || b.HitTest p) false thicknesses
    let tens = Array.fold(fun (s0:bool) (b:ThicknessButton) -> s0 || b.HitTest p) false thicknesses
    nav || prim || edit || thick || tens
    
  //converte direzione in coordinate
  let scrollBy dir =
    match dir with
    | NavBut.Up ->    (0.f, -10.f)
    | NavBut.Down ->  (0.f, 10.f)
    | NavBut.Left ->  (-10.f, 0.f)
    | NavBut.Right -> (10.f, 0.f)
    | _ -> (0.f,0.f)
  
  //handle dell'input da tastiera
  let handleCommand (k:Keys) =
    match k with
    | Keys.W -> scrollDir <- NavBut.Up
                scrollBy NavBut.Up |> translate
    | Keys.A -> scrollDir <- NavBut.Left
                scrollBy NavBut.Left |> translate
    | Keys.S -> scrollDir <- NavBut.Down
                scrollBy NavBut.Down |> translate
    | Keys.D -> scrollDir <- NavBut.Right
                scrollBy NavBut.Right |> translate
    | Keys.Q -> let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
                rotateAtW p 10.f
    | Keys.E -> let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
                rotateAtW p -10.f
    | Keys.Z -> let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
                scaleW(1.1f, 1.1f)
                let p1 = transformP v2w (Point(this.Width / 2, this.Height / 2))
                translateW(p1.X - p.X, p1.Y - p.Y)
    | Keys.X -> let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
                scaleW(1.f/1.1f, 1.f / 1.1f)
                let p1 = transformP v2w (Point(this.Width / 2, this.Height / 2))
                translateW(p1.X - p.X, p1.Y - p.Y)
    | Keys.L -> lineType <- PrimBut.Line;  newPrim <- true
    | Keys.C -> lineType <- PrimBut.Curve; newPrim <- true
    | Keys.B -> lineType <- PrimBut.Bezier;newPrim <- true
    | Keys.NumPad1 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.White.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.NumPad2 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.Gray.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.NumPad3 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.Black.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.NumPad4 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.Green.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.NumPad5 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.Purple.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.NumPad6 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.Orange.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.NumPad7 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.Red.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.NumPad8 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.Blue.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.NumPad9 -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(Pens.Yellow.Brush,prims.[prims.Length-1].Pen.Width)
    | Keys.H -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(prims.[prims.Length-1].Pen.Brush,1.f)
    | Keys.J -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(prims.[prims.Length-1].Pen.Brush,3.f)
    | Keys.K -> if prims.Length> 0 then prims.[prims.Length-1].Pen <- new Pen(prims.[prims.Length-1].Pen.Brush,5.f)
    | Keys.T -> if prims.Length> 0 then prims.[prims.Length-1].Tension <- prims.[prims.Length-1].Tension + 0.1f
    | Keys.Y -> if prims.Length> 0 then prims.[prims.Length-1].Tension <- prims.[prims.Length-1].Tension - 0.1f
    | _ -> ()
    this.Invalidate()
  
  //elimina l'elemento in posizione idx di a  
  let remove (idx:int) (a:PointF[]) : PointF[]=
    let mutable a1 = [||]
    for i = 0 to a.Length-1 do
        if i <> idx then
            a1 <- Array.append a1 [|pts.[i]|]
    a1
 

 //disegna gli hooks (rette di allineamento)
  let drawHook (g:Graphics) (p1:PointF) (p2:PointF) (t:single) (b:Brush)= 
     let pen = new Pen(b)
     pen.DashStyle <- DashStyle.Dot
      
     if abs(p2.X - p1.X) < t then
       let bx = PointF(p1.X,0.f)
       let ex = PointF(p1.X,single(this.Height))
       g.DrawLine(pen,bx,ex)

     if  abs(p2.Y - p1.Y) < t then
      let by = PointF(0.f,p1.Y)
      let ey = PointF(single(this.Width),p1.Y)
      g.DrawLine(pen,ey,by)
    
  do//double buffering
    this.SetStyle(ControlStyles.OptimizedDoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)
    
    //assegna i controlli all'editor
    navbar     |> Seq.iter (fun b -> b.Parent <- this; this.LWControls.Add(b))
    primitives |> Seq.iter (fun b -> b.Parent <- this; this.LWControls.Add(b))
    palette    |> Seq.iter (fun b -> b.Parent <- this; this.LWControls.Add(b))
    thicknesses|> Seq.iter (fun b -> b.Parent <- this; this.LWControls.Add(b))
    tensions   |> Seq.iter (fun b -> b.Parent <- this; this.LWControls.Add(b))

    //aggancio autoscroll
    scrollTimer.Tick.Add(fun _ ->
        scrollBy scrollDir |> translate
        this.Invalidate()
    )
    
    //aggiungo gli handler degli eventi ai LWCs dell'editor
    navbar.[int(NavBut.Up)].MouseDown.Add(fun _ -> handleCommand Keys.W)
    navbar.[int(NavBut.Down)].MouseDown.Add(fun _ -> handleCommand Keys.S)
    navbar.[int(NavBut.Left)].MouseDown.Add(fun _ -> handleCommand Keys.A)
    navbar.[int(NavBut.Right)].MouseDown.Add(fun _ ->  handleCommand Keys.D)
    navbar.[int(NavBut.Clock)].MouseDown.Add(fun _ ->  handleCommand Keys.E)
    navbar.[int(NavBut.AClock)].MouseDown.Add(fun _ ->  handleCommand Keys.Q)
    navbar.[int(NavBut.ZoomIn)].MouseDown.Add(fun _ ->  handleCommand Keys.Z)
    navbar.[int(NavBut.ZoomOut)].MouseDown.Add(fun _ ->  handleCommand Keys.X)
    
    primitives.[int(PrimBut.Line)].MouseDown.Add(fun _ -> handleCommand Keys.L)
    primitives.[int(PrimBut.Curve)].MouseDown.Add(fun _ -> handleCommand Keys.C)
    primitives.[int(PrimBut.Bezier)].MouseDown.Add(fun _ -> handleCommand Keys.B)

    palette.[int(ColBut.Red)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad7)
    palette.[int(ColBut.Blue)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad8)
    palette.[int(ColBut.Yellow)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad9)
    palette.[int(ColBut.Green)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad4)
    palette.[int(ColBut.Purple)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad5)
    palette.[int(ColBut.Orange)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad6)
    palette.[int(ColBut.White)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad1)
    palette.[int(ColBut.Gray)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad2)
    palette.[int(ColBut.Black)].MouseDown.Add(fun _ -> handleCommand Keys.NumPad3)
    
    thicknesses.[int(ThickBut.Thin)  ].MouseDown.Add(fun _ -> handleCommand Keys.H)
    thicknesses.[int(ThickBut.Normal)].MouseDown.Add(fun _ -> handleCommand Keys.J)
    thicknesses.[int(ThickBut.Thick) ].MouseDown.Add(fun _ -> handleCommand Keys.K)

    tensions.[int(TensBut.More)].MouseDown.Add(fun _ -> handleCommand Keys.T)
    tensions.[int(TensBut.Less) ].MouseDown.Add(fun _ -> handleCommand Keys.Y)

    for v in [ NavBut.Up; NavBut.Left; NavBut.Right; NavBut.Down] do
      let idx = int(v)
      navbar.[idx].MouseDown.Add(fun _ -> scrollTimer.Start())
      navbar.[idx].MouseUp.Add(fun _ -> scrollTimer.Stop())

 
 //DICHIARAZIONI PROPRIETA'
  member this.V2W = v2w

  //EVENTI DEL MOUSE
  override this.OnMouseDown e =
    base.OnMouseDown(e)
    let l = transformP v2w e.Location
    let ht = handleHitTest l

    selected <- pts |> Array.tryFindIndex ht

    //se premo il pulsante dx, rimuovo il punto selezionato
    // e le primitive non complete
    if e.Button = MouseButtons.Right then
        if not dragAndDrop then
            let samePoint (p1:PointF) (p2:PointF):bool=
                p1.X = p2.X && p1.Y = p2.Y

            match selected with
            | Some(idx) -> //rimuovo punto selezionato e scalo gli id dei punti
                           pts <- remove idx pts
                           for pr in prims do
                                pr.Points <- Array.filter (fun i -> i<>idx) pr.Points
                                pr.Points <- Array.map (fun i -> if i>idx then i-1 else i) pr.Points
                    
                           //printfn "RIMUOVO pts idx: %d" idx
                           //Array.iter (fun pr -> printf "%s" (pr.ToString())) prims
                           this.Invalidate()
            | _         -> newPrim <- true
    else
        //se c'e' un punto selzionato, mi preparo per il drag & drop
        match selected with
        | Some(idx) ->  let p = pts.[idx]
                        offsetDrag <- PointF(p.X - l.X, p.Y - l.Y)
                        dragAndDrop <- true
                        //printfn "pts idx: %d" idx
                        //printfn "primitive: %d" (trTable.Item(idx).Lt)
                        //printfn "primitive: %d" (trTable.Item(idx).Rt)

                        //se nessun punto è stato colpito, leggo il nuovo punto
        | None      ->  let p = new PointF(single(l.X),single(l.Y))
                        match this.Captured with
                        | None -> if newPrim then//nuova primitiva
                                      let mutable pr= new Primitive()
                                      newPrim <- false
                                      pr.Style  <- lineType
                                      pr.Points <- Array.append pr.Points [|pts.Length|]
                                      prims <- Array.append prims [|pr|]
                                  
                                  else  //aggiorno primitiva
                                      let mutable pps = prims.[prims.Length-1].Points
                                      prims.[prims.Length-1].Points <- Array.append pps [|pts.Length|]

                                  //Array.iter (fun pr -> printf "%s" (pr.ToString())) prims
                                  pts <- Array.append pts [|p|]
                                  this.Invalidate()
                        | Some(_) -> ()
  //onMouseUp
  override this.OnMouseUp e =
    base.OnMouseUp e
    selected <- None
    dragAndDrop <- false
    this.Invalidate()

  //OnMouseMove
  override this.OnMouseMove e =
    if e.Button = MouseButtons.Left then 
        let l = transformP v2w e.Location
        match selected with
        | Some idx -> pts.[idx] <- PointF(l.X + offsetDrag.X, l.Y + offsetDrag.Y)
                      this.Invalidate()
        | None -> () 

  
  //ON PAINT
  override this.OnPaint e =
    let g = e.Graphics
    let drawHandle (p:PointF) =
      let w = 5.f
      g.DrawEllipse(Pens.Black, p.X - w, p.Y - w, 2.f * w, 2.f * w)
    let ctx = g.Save()//salvo contesto grafico

    //imposto qualità
    g.SmoothingMode <- Drawing2D.SmoothingMode.HighQuality
    g.Transform <- w2v

    //disegno handles
    pts |> Array.iter drawHandle
    //disegno primitive
    for p in prims do
        p.draw g pts
    
    //disegno gli hooks
    if dragAndDrop then
        match selected with
        | Some(idx) -> //drawHook g pts.[idx] pts.[idx] 10.f Brushes.Blue
                       let pts0 = Array.filter (fun (p:PointF) -> p.X <> pts.[idx].X && p.Y <> pts.[idx].Y) pts
                       Array.iter (fun p -> drawHook g p pts.[idx] 10.f Brushes.DarkGray) pts0                            
        | _ -> ()

    g.Restore(ctx)//recupero contesto grafico precedentemente salvato
    base.OnPaint(e)
 

 //EVENTI DA TASTIERA
  override this.OnKeyDown e =
    handleCommand e.KeyCode
    

//---------------------//
//        MAIN         //
//---------------------//
let f = new Form(Text="Disegno Geometrico",TopMost=true,WindowState = FormWindowState.Maximized)
let e = new Editor(Dock=DockStyle.Fill)
f.Controls.Add(e)
e.Focus()
f.Show()