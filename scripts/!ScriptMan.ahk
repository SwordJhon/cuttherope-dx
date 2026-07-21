#Requires AutoHotkey v2
#SingleInstance Force
#NoTrayIcon
TraySetIcon("..\distribution\icons\CutTheRopeDXIcon.ico", , 1)
A_ScriptName := "ScriptMan" FigureOutProjectName()

; This will only work if A_WorkingDir is on the scripts folder lmao.
; Nothing will happen if it fails tho, so don't worry 'bout this.
FigureOutProjectName(*) {
    try {
        i := StrSplit(A_WorkingDir, "\")
        i.Pop
        return " (" i[i.Length] ")"
    } catch {
        return ""
    }
}

obj := ctrScriptMan()
obj.Show

; Basically all the code :P.
class ctrScriptMan {
    static ExePath := A_ScriptDir "\..\CutTheRopeDX\bin\Debug\net10.0\CutTheRope-DX.exe"
    Window := Gui("+DPIScale +Resize +MinSize300x200")
    Menu := {
        Window: unset,
        Command: unset,
        Macro: unset,
    }
    Output := {
        Tabs: unset,
        CMD: unset,
        BLD: unset,
    }
    StdOut := {
        PID: 0,
        TempFile: "",
        FilePos: 0,
        Leftover: "",
        OnDone: "",
        OutCtrl: "",
    }
    PollOutputCallback := ""

    ; Creates a new ScriptMan window.
    __New(*) {
        this.PollOutputCallback := this.PollOutput.Bind(this)
        this.Window.SetFont "S11"

        this.Menu.Window := Menu()
        this.Menu.Window.Add('&Clear output', (*) => this.ClearCurrentOutput())
        this.Menu.Window.Add('&Pin', (*) => WinSetAlwaysOnTop(-1, this.Window.Hwnd))
        this.Menu.Window.Add()
        this.Menu.Window.Add('&Open folder', (*) => Run(A_ScriptDir "/.."))
        this.Menu.Window.Add()
        this.Menu.Window.Add('&Reload', (*) => Reload())
        this.Menu.Window.Add('E&xit', (*) => ExitApp())

        this.Menu.Command := Menu()
        this.Menu.Command.Add('Generate release notes', (*) => this.RunScript("python -u generate_release_notes.py"))
        this.Menu.Command.Add('Bundle content', (*) => this.RunScript("python -u bundle_content.py"))
        this.Menu.Command.Add('Test build', (*) => this.RunScript("dotnet build -f net10.0", A_ScriptDir "\.."))

        this.Menu.Macro := Menu()
        this.Menu.Macro.Add('Standalone actions', this.Menu.Command)
        this.Menu.Macro.Add('Make a test build with new assets', (*) => this.RunScript("python -u bundle_content.py", ,
            (*) => this.RunScript("dotnet build -f net10.0", A_ScriptDir "\..")))

        this.Window.MenuBar := MenuBar()
        this.Window.MenuBar.Add('&Window...', this.Menu.Window)
        this.Window.MenuBar.Add('&Commands...', this.Menu.Macro)
        this.Window.MenuBar.Add('&Run test build', (*) => this.RunScript('"' ctrScriptMan.ExePath '"',
            A_ScriptDir, , this.Output.BLD))

        this.Output.Tabs := this.Window.AddTab3("-Wrap", ["Commands", "Test build"])
        this.Output.Tabs.UseTab 1
        this.Output.CMD := this.Window.AddEdit("r15 w500 ReadOnly +VScroll",
            "Choose one of the `"Commands...`" options to start.`n")
        this.Output.Tabs.UseTab 2
        this.Output.BLD := this.Window.AddEdit("r15 w500 ReadOnly +VScroll", "Click on `"Run test build`" to start.`n")
        if !FileExist(ctrScriptMan.ExePath) {
            this.Output.BLD.Value :=
                "A test build has not been found. Use `"Make a test build with new assets`" on the `"Commands...`" menu first.`n"
        }

        ; Needs to explicitly bind `this` or else it will lose it. Why must you be like this AHK...
        this.Window.OnEvent("Size", this.ChangeElementsSize.Bind(this))
    }

    ; Shows the window.
    Show(*) {
        this.Window.Show
    }

    ; Hides the window.
    Hide(*) {
        this.Window.Hide
    }

    ; Clears all output on the current tab.
    ClearCurrentOutput(*) {
        if (this.Output.Tabs.Value = 1) {
            this.Output.CMD.Value := ""
        } else {
            this.Output.BLD.Value := ""
        }
    }

    ; Starts cmd asynchronously and streams its output to outputCtrl as it runs.
    ; onDone fires once the process exits, letting commands be chained.
    RunScript(cmd, workDir := A_ScriptDir, onDone := "", outputCtrl := this.Output.CMD) {
        if this.StdOut.PID && ProcessExist(this.StdOut.PID) {
            outputCtrl.Value .= "[A command is already running -- please wait for it to finish.]`n"
            return
        }
        this.StdOut.TempFile := A_Temp "\scriptman_" A_TickCount ".txt"
        this.StdOut.FilePos := 0
        this.StdOut.Leftover := ""
        this.StdOut.OnDone := onDone
        this.StdOut.OutCtrl := outputCtrl

        Run(A_ComSpec ' /c "' cmd ' > "' this.StdOut.TempFile '" 2>&1"', workDir, "Hide", &pid)
        this.StdOut.PID := pid

        SetTimer(this.PollOutputCallback, 150)
    }

    ; Timer callback: reads any new output written since the last poll and appends it to the UI.
    ; Once the process exits, it stops the timer, flushes remaining output, cleans up the temp file, and fires onDone if one was given.
    PollOutput() {
        if FileExist(this.StdOut.TempFile) {
            try {
                f := FileOpen(this.StdOut.TempFile, "r", "UTF-8")
                f.Pos := this.StdOut.FilePos
                newText := f.Read()
                this.StdOut.FilePos := f.Pos
                f.Close()

                if (newText != "") {
                    newText := this.StdOut.Leftover . newText
                    lines := StrSplit(newText, "`n", "`r")
                    this.StdOut.Leftover := lines.Pop()
                    for line in lines {
                        if (line != "") {
                            this.StdOut.OutCtrl.Value .= line "`n"
                            ControlSend("^{End}", this.StdOut.OutCtrl)
                        }
                    }
                }
            } catch as err {
                this.StdOut.OutCtrl.Value .= "[poll error: " err.Message "]`n"
            }
        }

        if !ProcessExist(this.StdOut.PID) {
            SetTimer(this.PollOutputCallback, 0)
            if (this.StdOut.Leftover != "")
                this.StdOut.OutCtrl.Value .= this.StdOut.Leftover "`n"
            FileDelete(this.StdOut.TempFile)

            if (this.StdOut.OnDone != "") {
                callback := this.StdOut.OnDone
                this.StdOut.OnDone := ""
                callback()
            }
        }
    }

    ; Handles element resizing when resizing the window.
    ChangeElementsSize(GuiObj, MinMax, Width, Height) {
        if (MinMax = -1)
            return
        this.Output.Tabs.Move(, , Width - this.Window.MarginX * 2, Height - this.Window.MarginY * 2)
        this.Output.CMD.Move(, , Width - this.Window.MarginX * 4, Height - 11 - this.Window.MarginY * 6)
        this.Output.BLD.Move(, , Width - this.Window.MarginX * 4, Height - 11 - this.Window.MarginY * 6)
    }
}
