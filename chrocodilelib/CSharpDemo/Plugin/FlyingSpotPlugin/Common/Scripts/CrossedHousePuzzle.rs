init
{
	$SODX 264 256 265 82 67 65 66 69;
	$SHZ 50000;
}

fn main(scanFreq=10000)
{
	line(x1=-15.0000, y1=15.0000, x2=-15.0000, y2=-10.0000, numPts=2000,waitAtBegin=10000, label="line1")
	line(x1=-15.0000, y1=-10.0000, x2=0.0000, y2=-25.0000, numPts=2000,waitAtBegin=10000, label="line2")
	line(x1=0.0000, y1=-25.0000, x2=15.0000, y2=-10.0000, numPts=2000,waitAtBegin=10000, label="line3")
	line(x1=15.0000, y1=-10.0000, x2=-15.0000, y2=-10.0000, numPts=2000,waitAtBegin=10000, label="line4")
	line(x1=-15.0000, y1=-10.0000, x2=15.0000, y2=15.0000, numPts=2000,waitAtBegin=10000, label="line5")
	line(x1=15.0000, y1=15.0000, x2=-15.0000, y2=15.0000, numPts=2000,waitAtBegin=10000, label="line6")
	line(x1=-15.0000, y1=15.0000, x2=15.0000, y2=-10.0000, numPts=2000,waitAtBegin=10000, label="line7")
	line(x1=15.0000, y1=-10.0000, x2=15.0000, y2=15.0000, numPts=2000,waitAtBegin=10000, label="line8")
}