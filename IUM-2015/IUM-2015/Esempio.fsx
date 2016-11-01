
open System.Windows.Forms

let f = new Form(Text="Hello",TopMost=true)
f.Show()

let b = new WebBrowser(Dock=DockStyle.Fill)
b.Url <- System.Uri("http://www.unipi.it")

f.Controls.Add(b)

let addrbar = new Panel(Dock=DockStyle.Top)
f.Controls.Add(addrbar)

let text = new TextBox(Dock=DockStyle.Fill)
addrbar.Controls.Add(text)

addrbar.Height <- text.Height

let btn = new Button(Text="Go",Dock=DockStyle.Right)
addrbar.Controls.Add(btn)

btn.Click.Add(fun e ->
  b.Url <- System.Uri(text.Text)
)

b.Navigated.Add(fun e ->
  text.Text <- e.Url.ToString()
)

text.KeyUp.Add(fun e ->
  if e.KeyCode = Keys.Enter then
    b.Url <- System.Uri(text.Text)
)

