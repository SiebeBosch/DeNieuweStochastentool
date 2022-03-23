rem @ echo off
set OMP_NUM_THREADS=1

echo starting
call "c:\Program Files\Deltares\D-HYDRO Suite 2022.02 1D2D\plugins\DeltaShell.Dimr\kernels\x64\dimr\scripts\run_dimr.bat" "DIMR_Config.xml"
rem start /wait "c:\Program Files\Deltares\D-HYDRO Suite 2022.02 1D2D\plugins\DeltaShell.Dimr\kernels\x64\dimr\scripts\run_dimr.bat" "%~dp0\dimr_config.xml"
echo complete
rem pause


    rem To prevent the DOS box from disappearing immediately: remove the rem on the following line
rem pause
