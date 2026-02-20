#!/bin/bash
# Setup script for CHRocodile GUI Application Virtual Environment
# This script creates a virtual environment and installs dependencies

echo "Creating virtual environment..."
python3 -m venv venv

if [ $? -ne 0 ]; then
    echo "Error: Failed to create virtual environment"
    echo "Make sure Python 3.7+ is installed and accessible"
    exit 1
fi

echo ""
echo "Activating virtual environment..."
source venv/bin/activate

if [ $? -ne 0 ]; then
    echo "Error: Failed to activate virtual environment"
    exit 1
fi

echo ""
echo "Upgrading pip..."
python -m pip install --upgrade pip

echo ""
echo "Installing requirements..."
pip install -r requirements.txt

if [ $? -ne 0 ]; then
    echo "Error: Failed to install requirements"
    exit 1
fi

echo ""
echo "========================================"
echo "Setup complete!"
echo "========================================"
echo ""
echo "To activate the virtual environment in the future, run:"
echo "  source venv/bin/activate"
echo ""
echo "To run the application:"
echo "  python chrocodile_gui.py"
echo ""

