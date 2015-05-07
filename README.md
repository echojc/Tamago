# Tamago

Tamago is a .NET implementation of
[BulletML](http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/index_e.html) written
in C#.

This is the successor to my fork of
[BulletMLLib](https://github.com/echojc/BulletMLLib).

## Testing

Tests are written using NUnit. If you are using Visual Studio and have the
[NUnit test adapter](https://www.nuget.org/packages/NUnitTestAdapter/)
installed, you can run all tests by choosing *Test -> Run -> All Tests*.

## Differences with ABA Games implementation

There is a
[demo applet](http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml_applet_e.html)
on the official BulletML reference site. This implementation handles several
cases differently from that of the reference implementation.

### `<wait>`

ABA waits for an additional frame, such that

```
<wait>1</wait>
```

delays execution by 2 frames. This implementation waits for exactly the
specified number of frames.

### `<fire>`/`<bullet>`

ABA treats speed type `relative` identically to `sequence`, such that

```
<fire>
  <speed type="absolute">4</speed>
  <bullet>
    <action>
      <fire>
        <speed type="relative">2</speed>
        <bullet/>
      </fire>
    </action>
  </bullet>
</fire>
```

fires the second bullet at 3 pixels/frame (initial fire speed of `1` + `2`).
This implementation instead fires the second bullet at 6 pixels/frame (`4 + 2`).

### `<changeSpeed>`

ABA treats this task as a no-op if `<term>` is less than or equal to zero. This
implementation will always set the speed to the final value when a
`<changeSpeed>` task completes.

### `<changeDirection>`

Similarly, ABA treats this task as a no-op if `<term>` is less than or equal to
zero. This implementation will always set the direction to the final value when
a `<changeDirection>` task completes.

Further, when the direction type is `absolute`, `sequence`, or `relative`, ABA
does not alter the direction during the final frame of the task, such that

```
<fire>
  <direction type="absolute">180</direction>
  <bullet>
    <action>
      <changeDirection>
        <direction type="relative">90</direction>
        <term>3</term>
      </changeDirection>
    </action>
  </bullet>
</fire>
```

completes with the bullet pointing at -120 degrees (`180 + (90 * 2 / 3)`). In
this implementation, the bullet will end up pointing at -90 degrees.

In addition, when the direction type is `aim`, in place of running the final
frame ABA adds the current aim direction (to the player) to the bullet's
direction. Assuming the player is always directly below the bullet (so that the
aim direction is 180),

```
<fire>
  <direction type="absolute">90</direction>
  <bullet>
    <action>
      <changeDirection>
        <direction type="aim">0</direction>
        <term>2</term>
      </changeDirection>
    </action>
  </bullet>
</fire>
```

completes with the bullet's direction set to -45 degrees (`90 + 45 + 180`). In
this implementation, the bullet ends up pointing at 180 degrees.
