   M S B A M L   `   `   `  ????G  CApplication1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Application1Application1  5   	       Application1.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:Application1     5   	   -   5          ? Title$  Application1??5   	   $	??500??5	   	   $	??300??6   $	??500??5
   	   $	??300??6   $	??500??5   	   $	??300??6   .??5      ? .+?5   
   ??  ?? Padding$ 10q?5      ???	#FFBBBBBB._?5      ?  ? RowDefinitions 5      ?? 5      $
??Auto ?6$   ?? 5      $??* ?6$   .+?5      ? $??0??5      $??0,0,0,20q?5       ? ColumnDefinitions 5!      ?? 5"      $??* ?6+   ?? 5#      $
??Auto ?6+   .+?5&      ?? -   5         ??Name$ 
InputField??5+      
???   ?$??0??5(       ?? TextWrapping$
 Wrap=?5)       ?? AcceptsReturn . $??10q?5,      5-      ?? 5.      -   5         ??Name$ PublishButton??56      $??1??5/      ???	#FF24AFD8$??
10,10,0,10q?51      $??50??52      $??30??53      $??Publish??54      
???????58      59      ?? 5;      $??1q?5<      
???   ?$??1??5>      
???????._?5B      ?? -   5         ??Name$ ListOfResponses??5C      5D      5E      5F      5H      5J      5K      