# -*- coding: utf-8 -*-

import sys
import os

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
path_to_chr_pkg =  os.path.abspath(os.path.join(os.path.dirname(__file__), '..') + '/libcore')
sys.path.insert(0, path_to_chr_pkg)
# Required so that the default chrocodile ini is found where the chrpy package is located.
# os.chdir(path_to_chr_pkg)

import chrpy
