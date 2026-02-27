
fn drawCurve() {
	
	shape(label="curve") { // beginning of the scan box..
		let rX = 5, rY = 5
		moveTo(rX, 0)
		startMeasure()
		for i in range(0, 720*2) { 
			let a = i*M_PI/180
			let xx = rX*cos(a)*(sqrt(a+1)), yy = rY*sin(a)*(sqrt(a+1))
			//let xx = rX*cos(a), yy = rY*sin(a)
            moveTo(xx,yy)
		}
		stopMeasure()
	}
}

fn drawPoly() {

	let nPts = 500, N = 10, x0 = 0, y0 = 0
		
	shape(label = "poly") {
		for R in range(2, 6) {
			moveTo(x0 + R, y0)
			waitUsec(10000)
			startMeasure()
			for i in range(1, N+1) {
				let ang = M_PI*2*i / N
				let X = x0 + R*cos(ang), Y = y0 + R*sin(ang)
				lineTo(X, Y, nPts)
			}
			stopMeasure()
		}
	}
}


init
{
	$SODX 256 82 69 65 66;
	$SHZ 20000;
}

fn main(scanFreq=20000)
{
	drawCurve()
	drawPoly()

	let sizeX = 50, sizeY = 50
	rect(-sizeX, -sizeY, sizeX, sizeY, nrows=200, nCols=200, interp=1, scanHoriz=1, label="myfavoriterect")	
}
