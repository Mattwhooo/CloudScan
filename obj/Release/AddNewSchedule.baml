   M S B A M L   `   `   `  ����   	CloudScan    CloudScan.AddNewSchedule   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008       � Title$  AddNewProfile��$��530.225��$	��300�� � 
ResizeMode$ NoResize=�.���  � RowDefinitions �� $
��Auto ��� $��* ��� $��* ��� $��* ��� $��5* ��� $��2* ��� $��* �.+��� $
��Left=�.��Add New Schedule Profile� $��1�� � ColumnDefinitions �� $��* ��� $��7* �.+��� $��0��$��Center=�$
��URL:���� -    ��Name$ 
urlTextBox��$��20��$��1��$��0,6,10,7q� � ToolTip$) #Enter URL for Cloud without HTTP://��� $��2�� �� $��* ��� $��3* �.+��� $��0��$��Center=�$��	Username:��$��10,8,0,9q�$
��Left=�$��64���� -   $ usernameTextBox��$��20��$��1��$��	10,6,10,7q�$ Enter Cloud Username��� $��3�� �� $��* ��� $��3* �.+��� $��0��$��Center=�$��	Password:��$��10,9,0,9q��� -   $ passwordTextbox��$��20��$��1��$
��10,7q�$ Enter Cloud Password��� $��4�� � DataContext � -   listDataXml�� � XPath$ notifications/email�� � 	Resources    ScheduleListBoxDataTemplate��%    &	        �� .R��� ����  �� $��7* ��� $��* �.+��� 	 �� Orientation$	 Vertical=�.+��� $��10,0,0,0q�.��Notification List:�� -   $��ScheduleListBox��$��0��#��[  $	��191��$
��10,0q�$2 ,Email Address to receive error notifications������ 
 �� XPath$
 email���� $	 Vertical=�$��1��$��75��$��Stretch=�.+��� -   $
��Auto��$��0,0,10,0q�.��+�� �� �� -   $
��Auto��$��-��$��0,0,10,0q�� $��5�� �� $��2* ��� $��3* ��� $��* �.+��� $��0��$��Center=�$��Run Daily At: ��$��26��$
��0,30q��� $��1��$	 
Horizontal=�$��Center=�$��Center=�.+��� -   $��Hours��$��20��$
��0,28q�$��50���� $��Center=�.��:�� -	   $��Minutes��$��20��$
��0,28q�$��52���� $	 Vertical=�$��2��$��Center=�$��
0,28,10,28q�.+�	� -
   $��amRadio��$��AM��$��2��$
��Left=�$��Bottom=�	� -   $��pmRadio��$��PM��$��2��$
��Left=�$	��Top=�� $��6�� �� $��* ��� $��* �.+��� -   $��0��.��Add�� -   $��1��.��Cancel