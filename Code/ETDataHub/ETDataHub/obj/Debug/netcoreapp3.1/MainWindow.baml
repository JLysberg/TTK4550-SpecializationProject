   M S B A M L   `   `   `  ����D  @ETDataHub, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:ETDataHub	ETDataHub  5   	       ETDataHub.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:ETDataHub     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)    � WindowState$ 	Maximized=�65   .��5	      � .+�5
   
   �� -   5         ��Name$ imgStaticEnv��5
      $
��Fill=�6&   ]� 5   
   $��False��6   $��2��5      .+�5      �� -   5         ��Name$ 
tbETStatus��5      $��Bottom=�5      $��1��5      $��Not tracking��5       �� TextAlignment$ Center=�5      
���  ��$
��Wrap=�5      �� 5      -   5         ��Name$ spButtonsStatic��5      $y�	Collapsed=�5       �� Orientation$ 
Horizontal=�5      $
��Left=�5      $��Bottom=�5      $��Bottom=�5      .+�5      d� -   5         d�Name$ btnToggleET��5      $��0, 0, 10, 0q�5      $��Enable eye tracking��5      5      �� 5       -   5         ��Name$ btnLabelGaze��5       $��0, 0, 10, 0q�5!      $��Label gaze event��5"      5&      �� 5'      -   5        $ tbLabelGaze��5'      $��0, 0, 10, 0q�5(      
�������$.��(Left click: Fixation, Right click: Blink��5*      5+      �� 5,      -   5        $ spButtonsDynamic��5,      
�������$y�Visible=�5.      $ 
Horizontal=�5/      $
��Left=�50      $��Bottom=�51      $��Bottom=�52      .+�53      �� -   5        $ btnOpenDynamicEnv��53      $(��"Open dynamic labelling environment��54      55      �� 56      -	   5        	 ��Name$	 cbEnWarp��56      $
��Warp��57      $	��100��58      
 d� 	IsChecked$

 TrueY�59      ${�False��5:      �� 5;      -
   5        $	 cbEnLinMove��5;      $��LinMove��5<      $	��100��5=      $

 TrueY�5>      �� 5?      -   5        $	 cbEnQuadMove��5?      $��QuadMove��5@      $	��100��5A      $

 TrueY�5B      �� 5C      -   5        $	 cbEnCubicMove��5C      $��	CubicMove��5D      $	��100��5E      $
 FalseY�5F      5H      �� 5I      -   5        $ spEnvControls��5I      
�������$ Vertical=�5K      $
��Left=�5L      $	��Top=�5M      $	��Top=�5N      .+�5O      	� -   5         	�Name$ rbStaticEnv��5O      $��Static Environment��5P      $	��150��5Q      $
 FalseY�5R      5S      	� 5T      -   5        $ rbDynamicEnv��5T      $��Dynamic Environment��5U      $	��150��5V      $

 TrueY�5W      5X      5Y      5Z      5[      5\      