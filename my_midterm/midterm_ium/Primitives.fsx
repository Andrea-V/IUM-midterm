#load "Buttons.fsx"
open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D
open Buttons

//rappresenta una primitiva dell'editor
type Primitive() =
    //tratto
    let mutable pen    = Pens.Red
    
    //stile linea
    let mutable style  = PrimBut.Line
    
    //punti
    let mutable points = [||]
    
    //tensione (vuene usata sse style = PrimBut.Curve)
    let mutable tension= 0.5f

    //SETTERS & GETTERS
    member this.Tension
      with get() = tension
      and set(v) = tension <- v

    member this.Pen
      with get() = pen
      and set(v) = pen <- v

    member this.Style
      with get() = style
      and set(v) = style <- v

    member this.Points
      with get() = points
      and set(v) = points <- v
    
    //determina, in base al tipo di primitiva, quanti punti mancano per completarla
    member this.pts2get =
        match style with
        | PrimBut.Line  -> 2 - points.Length
        | PrimBut.Curve -> 3 - points.Length
        | PrimBut.Bezier-> if points.Length <4 then points.Length % 4 else (points.Length-4) % 3
        | _ -> 0
    
    //disegna la primitiva
    member this.draw (g:Graphics) (pts:PointF[]) =
        let p = new Pen(Brushes.DarkGray)
        p.DashStyle <- DashStyle.DashDot
        let pts0 = Array.map (fun i -> pts.[i]) this.Points
        
        match this.Style with
        | PrimBut.Line  -> if this.pts2get <= 0 then g.DrawLines(this.Pen,pts0)
        | PrimBut.Curve -> if this.pts2get <= 0 then g.DrawCurve(this.Pen,pts0,this.Tension)
        | PrimBut.Bezier-> let mutable i=0
                           while i <= pts0.Length-4 do
                                g.DrawLine(p,pts0.[i+0],pts0.[i+1])
                                g.DrawLine(p,pts0.[i+2],pts0.[i+3])
                                g.DrawBezier(this.Pen,pts0.[i+0],pts0.[i+1],pts0.[i+2],pts0.[i+3])
                                i <- i+3
        | _ -> ()
    
    //ToString
    override this.ToString() =
       "["+(Array.fold (fun s p -> s+" "+p.ToString() ) "" points) + "]\n"
    