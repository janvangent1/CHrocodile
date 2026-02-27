init
{
	$SODX 264 256 265 82 67 65 66 69;
	$SHZ 50000;
}

fn main(scanFreq=50000)
{
	line(x1=-12.5246, y1=6.7049, x2=0.0437, y2=-10.9180, numPts=5000,waitAtBegin=10000, label="line1")
	line(x1=0.0437, y1=-10.9180, x2=13.7049, y2=6.7049, numPts=5000,waitAtBegin=10000, label="line2")
	line(x1=-12.5246, y1=6.5683, x2=13.7049, y2=6.7049, numPts=260,waitAtBegin=10000, label="line3")
}

