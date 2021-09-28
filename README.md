# ConsoleControlLib
A library to create WinForm like controls on .NET console apps

### DISCLAIMER : This is a proof of concept. The performances are really bad and it was made to play around with it. I will maybe do one day a true library with great performaces

## Examples :

Global usage :
```
using ConsoleControls;

static ConsoleUI consoleUI = new ConsoleUI();   //Declare the console "form" itself
static CCButton ccb;                            //Button control
static CCLabel ccl;                             //Label control
static CCProgressBar ccp;

static void Main(string[] args)
{
    consoleUI.ReadyConsole();                   //Ready the console to welcome the controls
    
    ccl = new CCLabel();                        //Create a label control 
    ccl.Text = "I'm a label";                   //Set the text
    ccl.Border = BorderStyle.NoBorder;          //Disable the border
    consoleUI.Controls.Add(ccl);                //Add it to the console
    
    ccb = new CCButton();
    ccb.Text = "Click on me";
    ccb.Top = 5;                                //Offset the button to not overlap with the label
    ccb.Border = BorderStyle.FullBorder;
    ccb.Click += Button_Click;
    consoleUI.Controls.Add(ccb);
    
    ccp = new CCProgressBar();
    ccp.Top = 10
    consoleUI.Controls.Add(ccp);
}

private static void Button_Click(object sender, EventArgs e)
{
    ccp.Value += 2.5F;
}
```

Go to the wiki page for a complete usage
