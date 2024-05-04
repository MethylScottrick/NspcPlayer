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
		Me.btnOpen = New System.Windows.Forms.Button()
		Me.tbSeek = New System.Windows.Forms.TrackBar()
		Me.lblFile = New System.Windows.Forms.Label()
		Me.lblGame = New System.Windows.Forms.Label()
		Me.lblSongIndex = New System.Windows.Forms.Label()
		Me.cboSongIndices = New System.Windows.Forms.ComboBox()
		Me.btnRepeat = New System.Windows.Forms.Button()
		Me.btnStop = New System.Windows.Forms.Button()
		Me.btnPause = New System.Windows.Forms.Button()
		Me.btnPlay = New System.Windows.Forms.Button()
		Me.lblGameLabel = New System.Windows.Forms.Label()
		CType(Me.tbSeek, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'btnOpen
		'
		Me.btnOpen.Location = New System.Drawing.Point(16, 16)
		Me.btnOpen.Name = "btnOpen"
		Me.btnOpen.Size = New System.Drawing.Size(64, 24)
		Me.btnOpen.TabIndex = 0
		Me.btnOpen.Text = "Open..."
		Me.btnOpen.UseVisualStyleBackColor = True
		'
		'tbSeek
		'
		Me.tbSeek.Enabled = False
		Me.tbSeek.Location = New System.Drawing.Point(8, 128)
		Me.tbSeek.Name = "tbSeek"
		Me.tbSeek.Size = New System.Drawing.Size(440, 45)
		Me.tbSeek.TabIndex = 1
		'
		'lblFile
		'
		Me.lblFile.Location = New System.Drawing.Point(96, 16)
		Me.lblFile.Name = "lblFile"
		Me.lblFile.Size = New System.Drawing.Size(352, 24)
		Me.lblFile.TabIndex = 2
		Me.lblFile.Text = "(no file)"
		Me.lblFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		'
		'lblGame
		'
		Me.lblGame.AutoSize = True
		Me.lblGame.Location = New System.Drawing.Point(96, 56)
		Me.lblGame.Name = "lblGame"
		Me.lblGame.Size = New System.Drawing.Size(0, 13)
		Me.lblGame.TabIndex = 9
		'
		'lblSongIndex
		'
		Me.lblSongIndex.AutoSize = True
		Me.lblSongIndex.Location = New System.Drawing.Point(16, 88)
		Me.lblSongIndex.Name = "lblSongIndex"
		Me.lblSongIndex.Size = New System.Drawing.Size(64, 13)
		Me.lblSongIndex.TabIndex = 10
		Me.lblSongIndex.Text = "Song Index:"
		'
		'cboSongIndices
		'
		Me.cboSongIndices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.cboSongIndices.FormattingEnabled = True
		Me.cboSongIndices.Location = New System.Drawing.Point(88, 88)
		Me.cboSongIndices.Name = "cboSongIndices"
		Me.cboSongIndices.Size = New System.Drawing.Size(48, 21)
		Me.cboSongIndices.TabIndex = 11
		'
		'btnRepeat
		'
		Me.btnRepeat.Image = Global.NspcPlayer.My.Resources.Resources.btn24RepeatOn
		Me.btnRepeat.Location = New System.Drawing.Point(288, 88)
		Me.btnRepeat.Name = "btnRepeat"
		Me.btnRepeat.Size = New System.Drawing.Size(32, 32)
		Me.btnRepeat.TabIndex = 8
		Me.btnRepeat.UseVisualStyleBackColor = True
		'
		'btnStop
		'
		Me.btnStop.Enabled = False
		Me.btnStop.Image = Global.NspcPlayer.My.Resources.Resources.btn24Stop
		Me.btnStop.Location = New System.Drawing.Point(248, 88)
		Me.btnStop.Name = "btnStop"
		Me.btnStop.Size = New System.Drawing.Size(32, 32)
		Me.btnStop.TabIndex = 7
		Me.btnStop.UseVisualStyleBackColor = True
		'
		'btnPause
		'
		Me.btnPause.Enabled = False
		Me.btnPause.Image = Global.NspcPlayer.My.Resources.Resources.btn24Pause
		Me.btnPause.Location = New System.Drawing.Point(208, 88)
		Me.btnPause.Name = "btnPause"
		Me.btnPause.Size = New System.Drawing.Size(32, 32)
		Me.btnPause.TabIndex = 5
		Me.btnPause.UseVisualStyleBackColor = True
		'
		'btnPlay
		'
		Me.btnPlay.Enabled = False
		Me.btnPlay.Image = Global.NspcPlayer.My.Resources.Resources.btn24Play
		Me.btnPlay.Location = New System.Drawing.Point(168, 88)
		Me.btnPlay.Name = "btnPlay"
		Me.btnPlay.Size = New System.Drawing.Size(32, 32)
		Me.btnPlay.TabIndex = 4
		Me.btnPlay.UseVisualStyleBackColor = True
		'
		'lblGameLabel
		'
		Me.lblGameLabel.AutoSize = True
		Me.lblGameLabel.Location = New System.Drawing.Point(40, 56)
		Me.lblGameLabel.Name = "lblGameLabel"
		Me.lblGameLabel.Size = New System.Drawing.Size(38, 13)
		Me.lblGameLabel.TabIndex = 12
		Me.lblGameLabel.Text = "Game:"
		'
		'frmPlayer
		'
		Me.AllowDrop = True
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(458, 189)
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
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
		Me.MaximizeBox = False
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

End Class
