# WebREPLNET

# WebREPL Client
A implementation of MicroPythons WebREPL from .NET
### Introduction

### Usage example

Include the WebREPLClient project into the solution and initialize it

```
	...
	client = new WebREPLConnection();
	client.DataReceived += Client_DataReceived;
	client.FileRecieved += Client_FileRecieved;
	client.UpdateStatus += Client_UpdateStatus;
	...	
```

Connect to the WebREPL device
```
	...
	client.Connect("ws://192.168.4.1:8266/");  
	...
```

Handle the incoming data:

```
	private void Client_DataReceived(object sender, WebREPLConnection.DataReceivedEventArgs e)
	{
		Console.WriteLine(e.Data);
	}
```

Get a file:
```
	...
	client.get_file("main.py");
	...

	private void Client_FileRecieved(object sender, WebREPLConnection.FileReceivedEventArgs e)
	{
		// Invoke in UI Thread
		this.Invoke((MethodInvoker)(() =>
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.FileName = e.Filename;
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				System.IO.File.WriteAllBytes(dialog.FileName, e.Data);
			}
		}));
	}
	
```

Send a file:
```
	private void SendFileButton_Click(object sender, EventArgs e)
	{
		OpenFileDialog dialog = new OpenFileDialog();
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			string remoteFilename = System.IO.Path.GetFileName(dialog.FileName);
			byte[] data = System.IO.File.ReadAllBytes(dialog.FileName);
			client.put_file(remoteFilename, data);
		}
	}
```
