#!/bin/bash
version=$(grep -oP -m 1 '\* \K[0-9]*\.[0-9]*\.[0-9]*' ReleaseNotes3.md)
export version