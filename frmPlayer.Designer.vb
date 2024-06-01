<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPlayer
	Inherits System.Windows.Forms.Form

	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer

	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Dim lblTimeLabel As System.Windows.Forms.Label
		Me.btnOpen = New System.Windows.Forms.Button()
		Me.tbSeek = New System.Windows.Forms.TrackBar()
		Me.lblFile = New System.Windows.Forms.Label()
		Me.lblGame = New System.Windows.Forms.Label()
		Me.lblSongIndex = New System.Windows.Forms.Label()
		Me.cboSongIndices = New System.Windows.Forms.ComboBox()
		Me.lblGameLabel = New System.Windows.Forms.Label()
		Me.btnRepeat = New System.Windows.Forms.Button()
		Me.btnStop = New System.Windows.Forms.Button()
		Me.btnPause = New System.Windows.Forms.Button()
		Me.btnPlay = New System.Windows.Forms.Button()
		Me.btnRewind = New System.Windows.Forms.Button()
		Me.btnFastForward = New System.Windows.Forms.Button()
		Me.pnlTransportAnchor = New System.Windows.Forms.Panel()
		Me.lblTime = New System.Windows.Forms.Label()
		Me.lblDebug = New System.Windows.Forms.Label()
		Me.timUpdateDebugInfo = New System.Windows.Forms.Timer(Me.components)
		Me.btnExport = New System.Windows.Forms.Button()
		Me.timUpdateTime = New System.Windows.Forms.Timer(Me.components)
		lblTimeLabel = New System.Windows.Forms.Label()
		CType(Me.tbSeek, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'btnOpen
		'
		Me.btnOpen.Location = New System.Drawing.Point(16, 16)
		Me.btnOpen.Name = "btnOpen"
		Me.btnOpen.Size = New System.Drawing.Size(56, 24)
		Me.btnOpen.TabIndex = 0
		Me.btnOpen.Text = "Open..."
		Me.btnOpen.UseVisualStyleBackColor = True
		'
		'tbSeek
		'
		Me.tbSeek.Enabled = False
		Me.tbSeek.Location = New System.Drawing.Point(8, 152)
		Me.tbSeek.Name = "tbSeek"
		Me.tbSeek.Size = New System.Drawing.Size(440, 45)
		Me.tbSeek.TabIndex = 16
		Me.tbSeek.Visible = False
		'
		'lblFile
		'
		Me.lblFile.Location = New System.Drawing.Point(160, 16)
		Me.lblFile.Name = "lblFile"
		Me.lblFile.Size = New System.Drawing.Size(288, 24)
		Me.lblFile.TabIndex = 2
		Me.lblFile.Text = "(no file)"
		Me.lblFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		'
		'lblGame
		'
		Me.lblGame.AutoSize = True
		Me.lblGame.Location = New System.Drawing.Point(88, 56)
		Me.lblGame.Name = "lblGame"
		Me.lblGame.Size = New System.Drawing.Size(0, 13)
		Me.lblGame.TabIndex = 4
		'
		'lblSongIndex
		'
		Me.lblSongIndex.AutoSize = True
		Me.lblSongIndex.Location = New System.Drawing.Point(16, 88)
		Me.lblSongIndex.Name = "lblSongIndex"
		Me.lblSongIndex.Size = New System.Drawing.Size(64, 13)
		Me.lblSongIndex.TabIndex = 5
		Me.lblSongIndex.Text = "Song Index:"
		'
		'cboSongIndices
		'
		Me.cboSongIndices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cboSongIndices.FormattingEnabled = True
		Me.cboSongIndices.Location = New System.Drawing.Point(88, 88)
		Me.cboSongIndices.Name = "cboSongIndices"
		Me.cboSongIndices.Size = New System.Drawing.Size(48, 21)
		Me.cboSongIndices.TabIndex = 6
		'
		'lblGameLabel
		'
		Me.lblGameLabel.AutoSize = True
		Me.lblGameLabel.Location = New System.Drawing.Point(40, 56)
		Me.lblGameLabel.Name = "lblGameLabel"
		Me.lblGameLabel.Size = New System.Drawing.Size(38, 13)
		Me.lblGameLabel.TabIndex = 3
		Me.lblGameLabel.Text = "Game:"
		'
		'btnRepeat
		'
		Me.btnRepeat.Image = Global.NspcPlayer.My.Resources.Resources.btn24RepeatOn
		Me.btnRepeat.Location = New System.Drawing.Point(328, 88)
		Me.btnRepeat.Name = "btnRepeat"
		Me.btnRepeat.Size = New System.Drawing.Size(32, 32)
		Me.btnRepeat.TabIndex = 12
		Me.btnRepeat.UseVisualStyleBackColor = True
		Me.btnRepeat.Visible = False
		'
		'btnStop
		'
		Me.btnStop.Enabled = False
		Me.btnStop.Image = Global.NspcPlayer.My.Resources.Resources.btn24Stop
		Me.btnStop.Location = New System.Drawing.Point(288, 88)
		Me.btnStop.Name = "btnStop"
		Me.btnStop.Size = New System.Drawing.Size(32, 32)
		Me.btnStop.TabIndex = 11
		Me.btnStop.UseVisualStyleBackColor = True
		'
		'btnPause
		'
		Me.btnPause.Enabled = False
		Me.btnPause.Image = Global.NspcPlayer.My.Resources.Resources.btn24Pause
		Me.btnPause.Location = New System.Drawing.Point(248, 88)
		Me.btnPause.Name = "btnPause"
		Me.btnPause.Size = New System.Drawing.Size(32, 32)
		Me.btnPause.TabIndex = 10
		Me.btnPause.UseVisualStyleBackColor = True
		'
		'btnPlay
		'
		Me.btnPlay.Enabled = False
		Me.btnPlay.Image = Global.NspcPlayer.My.Resources.Resources.btn24Play
		Me.btnPlay.Location = New System.Drawing.Point(208, 88)
		Me.btnPlay.Name = "btnPlay"
		Me.btnPlay.Size = New System.Drawing.Size(32, 32)
		Me.btnPlay.TabIndex = 9
		Me.btnPlay.UseVisualStyleBackColor = True
		'
		'btnRewind
		'
		Me.btnRewind.Enabled = False
		Me.btnRewind.Image = Global.NspcPlayer.My.Resources.Resources.btn24Rewind
		Me.btnRewind.Location = New System.Drawing.Point(168, 88)
		Me.btnRewind.Name = "btnRewind"
		Me.btnRewind.Size = New System.Drawing.Size(32, 32)
		Me.btnRewind.TabIndex = 8
		Me.btnRewind.UseVisualStyleBackColor = True
		'
		'btnFastForward
		'
		Me.btnFastForward.Enabled = False
		Me.btnFastForward.Image = Global.NspcPlayer.My.Resources.Resources.btn24FastForward
		Me.btnFastForward.Location = New System.Drawing.Point(368, 88)
		Me.btnFastForward.Name = "btnFastForward"
		Me.btnFastForward.Size = New System.Drawing.Size(32, 32)
		Me.btnFastForward.TabIndex = 13
		Me.btnFastForward.UseVisualStyleBackColor = True
		'
		'pnlTransportAnchor
		'
		Me.pnlTransportAnchor.Enabled = False
		Me.pnlTransportAnchor.Location = New System.Drawing.Point(168, 88)
		Me.pnlTransportAnchor.Name = "pnlTransportAnchor"
		Me.pnlTransportAnchor.Size = New System.Drawing.Size(8, 8)
		Me.pnlTransportAnchor.TabIndex = 7
		Me.pnlTransportAnchor.Visible = False
		'
		'lblTime
		'
		Me.lblTime.AutoSize = True
		Me.lblTime.Location = New System.Drawing.Point(88, 120)
		Me.lblTime.Name = "lblTime"
		Me.lblTime.Size = New System.Drawing.Size(0, 13)
		Me.lblTime.TabIndex = 15
		'
		'lblDebug
		'
		Me.lblDebug.AutoSize = True
		Me.lblDebug.Location = New System.Drawing.Point(8, 192)
		Me.lblDebug.Name = "lblDebug"
		Me.lblDebug.Size = New System.Drawing.Size(0, 13)
		Me.lblDebug.TabIndex = 17
		'
		'timUpdateDebugInfo
		'
		Me.timUpdateDebugInfo.Interval = 25
		'
		'btnExport
		'
		Me.btnExport.Enabled = False
		Me.btnExport.Location = New System.Drawing.Point(88, 16)
		Me.btnExport.Name = "btnExport"
		Me.btnExport.Size = New System.Drawing.Size(56, 24)
		Me.btnExport.TabIndex = 1
		Me.btnExport.Text = "Export..."
		Me.btnExport.UseVisualStyleBackColor = True
		'
		'timUpdateTime
		'
		Me.timUpdateTime.Interval = 50
		'
		'lblTimeLabel
		'
		lblTimeLabel.AutoSize = True
		lblTimeLabel.Location = New System.Drawing.Point(40, 120)
		lblTimeLabel.Name = "lblTimeLabel"
		lblTimeLabel.Size = New System.Drawing.Size(33, 13)
		lblTimeLabel.TabIndex = 14
		lblTimeLabel.Text = "Time:"
		'
		'frmPlayer
		'
		Me.AllowDrop = True
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.AutoSize = True
		Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
		Me.ClientSize = New System.Drawing.Size(458, 213)
		Me.Controls.Add(lblTimeLabel)
		Me.Controls.Add(Me.btnExport)
		Me.Controls.Add(Me.lblDebug)
		Me.Controls.Add(Me.lblTime)
		Me.Controls.Add(Me.btnFastForward)
		Me.Controls.Add(Me.lblGameLabel)
		Me.Controls.Add(Me.cboSongIndices)
		Me.Controls.Add(Me.lblSongIndex)
		Me.Controls.Add(Me.lblGame)
		Me.Controls.Add(Me.btnRepeat)
		Me.Controls.Add(Me.btnStop)
		Me.Controls.Add(Me.btnPause)
		Me.Controls.Add(Me.btnPlay)
		Me.Controls.Add(Me.lblFile)
		Me.Controls.Add(Me.tbSeek)
		Me.Controls.Add(Me.btnOpen)
		Me.Controls.Add(Me.btnRewind)
		Me.Controls.Add(Me.pnlTransportAnchor)
		Me.MaximizeBox = False
		Me.MinimumSize = New System.Drawing.Size(474, 200)
		Me.Name = "frmPlayer"
		Me.Text = "N-SPC Player"
		CType(Me.tbSeek, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents btnOpen As System.Windows.Forms.Button
	Friend WithEvents tbSeek As System.Windows.Forms.TrackBar
	Friend WithEvents lblFile As System.Windows.Forms.Label
	Friend WithEvents btnPlay As System.Windows.Forms.Button
	Friend WithEvents btnPause As System.Windows.Forms.Button
	Friend WithEvents btnStop As System.Windows.Forms.Button
	Friend WithEvents btnRepeat As System.Windows.Forms.Button
	Friend WithEvents lblGame As System.Windows.Forms.Label
	Friend WithEvents lblSongIndex As System.Windows.Forms.Label
	Friend WithEvents cboSongIndices As System.Windows.Forms.ComboBox
	Friend WithEvents lblGameLabel As System.Windows.Forms.Label
	Friend WithEvents btnRewind As System.Windows.Forms.Button
	Friend WithEvents btnFastForward As System.Windows.Forms.Button
	Friend WithEvents pnlTransportAnchor As System.Windows.Forms.Panel
	Friend WithEvents lblTime As System.Windows.Forms.Label
	Friend WithEvents lblDebug As System.Windows.Forms.Label
	Friend WithEvents timUpdateDebugInfo As System.Windows.Forms.Timer
	Friend WithEvents btnExport As System.Windows.Forms.Button
	Friend WithEvents timUpdateTime As System.Windows.Forms.Timer

End Class
